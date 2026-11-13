import { beforeEach, describe, expect, it } from 'vitest';
import { createAccessToken } from '@/shared/demo/demoAuth';
import { DemoHubConnection, type DemoHubDeps } from '@/shared/demo/demoHub';
import { createSeedDatabase } from '@/shared/demo/demoSeed';
import type { DemoPatientRecord } from '@/shared/demo/demoTypes';
import type { RiskStatusPayload, VitalsTickPayload } from '@/shared/types/api';
import { FIXED_NOW } from '@/test/demoTestUtils';

const PATIENTS = createSeedDatabase(FIXED_NOW).patients;

/** A hand-cranked clock + scheduler, so streaming can be tested without waiting in real time. */
function createScheduler(startMs: number) {
  let nowMs = startMs;
  let callback: (() => void) | null = null;
  let clearCalls = 0;
  return {
    advance(ms: number, stepMs = 40): void {
      for (let elapsed = 0; elapsed < ms; elapsed += stepMs) {
        nowMs += stepMs;
        callback?.();
      }
    },
    isRunning: () => callback !== null,
    clearCalls: () => clearCalls,
    deps(extra: Partial<DemoHubDeps> = {}): Partial<DemoHubDeps> {
      return {
        now: () => nowMs,
        setInterval: (cb: () => void) => {
          callback = cb;
          return 1;
        },
        clearInterval: () => {
          callback = null;
          clearCalls += 1;
        },
        getPatient: (id: string) => PATIENTS.find((patient) => patient.id === id) ?? null,
        ...extra,
      };
    },
  };
}

const roleToken = (role: 'Patient' | 'Doctor') =>
  createAccessToken(
    role === 'Patient' ? { sub: 'u', role, patientId: 'demo-patient-3' } : { sub: 'u', role, doctorId: 'demo-doctor-1' },
    FIXED_NOW,
  );

let scheduler: ReturnType<typeof createScheduler>;

beforeEach(() => {
  scheduler = createScheduler(FIXED_NOW);
});

describe('DemoHubConnection', () => {
  it('streams ECG at 250 samples per second while someone is listening', async () => {
    const hub = new DemoHubConnection(() => null, scheduler.deps());
    await hub.start();
    const samples: number[] = [];
    hub.on('ecg:demo-patient-1', (sample: number) => samples.push(sample));

    expect(scheduler.isRunning()).toBe(true);
    scheduler.advance(1000);
    expect(samples).toHaveLength(250);
    expect(samples.every(Number.isFinite)).toBe(true);
  });

  it('keeps the exact rate over a long run even if ticks arrive irregularly', async () => {
    const hub = new DemoHubConnection(() => null, scheduler.deps());
    await hub.start();
    let count = 0;
    hub.on('ecg:demo-patient-1', () => {
      count += 1;
    });
    scheduler.advance(3000, 40);
    scheduler.advance(3000, 130); // slower ticks emit bigger bursts
    expect(Math.abs(count - 1500)).toBeLessThanOrEqual(33);
  });

  it('pushes vitals every 2 seconds and a risk update every 10 seconds', async () => {
    const hub = new DemoHubConnection(() => roleToken('Doctor'), scheduler.deps());
    await hub.start();
    const vitals: VitalsTickPayload[] = [];
    const risks: RiskStatusPayload[] = [];
    hub.on('vitals:demo-patient-3', (payload: VitalsTickPayload) => vitals.push(payload));
    hub.on('riskStatus:demo-patient-3', (payload: RiskStatusPayload) => risks.push(payload));

    scheduler.advance(10_040);
    expect(vitals).toHaveLength(5);
    expect(vitals[0]?.patientId).toBe('demo-patient-3');
    expect(vitals.every((tick) => tick.heartRate > 90 && tick.spo2 <= 95)).toBe(true); // the critical patient
    expect(risks).toHaveLength(1);
    expect(risks[0]?.statusLevel).toBe('Critical');
    expect(typeof risks[0]?.riskScore).toBe('number'); // doctors get the score
  });

  it('never sends the risk score, confidence or flags to a patient viewer', async () => {
    const hub = new DemoHubConnection(() => roleToken('Patient'), scheduler.deps());
    await hub.start();
    const risks: RiskStatusPayload[] = [];
    hub.on('riskStatus:demo-patient-3', (payload: RiskStatusPayload) => risks.push(payload));
    scheduler.advance(10_040);

    expect(risks).toHaveLength(1);
    expect(risks[0]?.statusLevel).toBe('Critical');
    expect(risks[0]?.riskScore).toBeUndefined();
    expect(risks[0]?.confidenceScore).toBe(0);
    expect(risks[0]?.flags).toEqual([]);
  });

  it('stops the stream and its timer when the last handler is removed', async () => {
    const hub = new DemoHubConnection(() => null, scheduler.deps());
    await hub.start();
    const first = () => undefined;
    const second = () => undefined;
    hub.on('ecg:demo-patient-1', first);
    hub.on('vitals:demo-patient-1', second);

    hub.off('ecg:demo-patient-1', first);
    expect(scheduler.isRunning()).toBe(true); // vitals handler still registered
    hub.off('vitals:demo-patient-1', second);
    expect(scheduler.isRunning()).toBe(false);
    expect(scheduler.clearCalls()).toBe(1);
  });

  it('shares one stream between several handlers and several patients', async () => {
    const hub = new DemoHubConnection(() => null, scheduler.deps());
    await hub.start();
    const a: number[] = [];
    const b: number[] = [];
    const other: number[] = [];
    hub.on('ecg:demo-patient-1', (sample: number) => a.push(sample));
    hub.on('ecg:demo-patient-1', (sample: number) => b.push(sample));
    hub.on('ecg:demo-patient-2', (sample: number) => other.push(sample));
    scheduler.advance(400); // 10 ticks of 40 ms, 10 samples each
    expect(a).toHaveLength(100);
    expect(a).toEqual(b);
    expect(other).toHaveLength(100);
    expect(other).not.toEqual(a);
  });

  it('streams nothing for an offline device or a patient without a device', async () => {
    const template = PATIENTS[0];
    if (!template) throw new Error('seed has no patients');
    const idle: DemoPatientRecord = { ...template, id: 'idle-patient', scenario: 'idle' };
    const hub = new DemoHubConnection(
      () => null,
      scheduler.deps({
        getPatient: (id: string) => [...PATIENTS, idle].find((patient) => patient.id === id) ?? null,
      }),
    );
    await hub.start();
    const received: unknown[] = [];
    hub.on('ecg:demo-patient-8', (sample: number) => received.push(sample)); // seeded offline device
    hub.on('ecg:idle-patient', (sample: number) => received.push(sample));
    hub.on('ecg:no-such-patient', (sample: number) => received.push(sample));

    expect(scheduler.isRunning()).toBe(false);
    scheduler.advance(1000);
    expect(received).toHaveLength(0);
  });

  it('starts streams that were requested before the connection opened', async () => {
    const hub = new DemoHubConnection(() => null, scheduler.deps());
    const samples: number[] = [];
    hub.on('ecg:demo-patient-1', (sample: number) => samples.push(sample));
    expect(scheduler.isRunning()).toBe(false); // not connected yet
    await hub.start();
    expect(scheduler.isRunning()).toBe(true);
    scheduler.advance(200);
    expect(samples).toHaveLength(50);
  });

  it('stops everything on stop() and ignores a start() that was superseded', async () => {
    const hub = new DemoHubConnection(() => null, scheduler.deps());
    await hub.start();
    const samples: number[] = [];
    hub.on('ecg:demo-patient-1', (sample: number) => samples.push(sample));
    await hub.stop();
    expect(scheduler.isRunning()).toBe(false);
    scheduler.advance(400);
    expect(samples).toHaveLength(0);
    expect(hub.state).toBe('Disconnected');

    const second = new DemoHubConnection(() => null, scheduler.deps());
    second.on('ecg:demo-patient-1', () => undefined);
    const starting = second.start();
    await second.stop(); // React StrictMode: the effect is cleaned up while still "connecting"
    await starting;
    expect(second.state).toBe('Disconnected');
    expect(scheduler.isRunning()).toBe(false);
  });

  it('answers the join/leave calls and rejects unknown hub methods', async () => {
    const hub = new DemoHubConnection(() => null, scheduler.deps());
    await expect(hub.invoke('JoinPatientGroup')).resolves.toBeUndefined();
    await expect(hub.invoke('LeavePatientGroup')).resolves.toBeUndefined();
    await expect(hub.invoke('DropDatabase')).rejects.toThrow();
  });

  it('keeps streaming when one handler throws', async () => {
    const hub = new DemoHubConnection(() => null, scheduler.deps());
    await hub.start();
    const healthy: number[] = [];
    hub.on('ecg:demo-patient-1', () => {
      throw new Error('boom');
    });
    hub.on('ecg:demo-patient-1', (sample: number) => healthy.push(sample));

    // The hub logs each failure; silence that expected noise while the test runs.
    const originalConsoleError = console.error;
    console.error = () => undefined;
    try {
      scheduler.advance(200);
    } finally {
      console.error = originalConsoleError;
    }
    expect(healthy).toHaveLength(50);
  });
});
