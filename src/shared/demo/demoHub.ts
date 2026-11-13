/**
 * File: src/shared/demo/demoHub.ts
 * Layer: shared/demo (in-browser demo backend)
 * Purpose: A stand-in for the SignalR HubConnection that pushes SIMULATED live data (ECG samples, heart-rate /
 *          SpO2 ticks, risk-status updates) to the dashboards with no server involved.
 * Logic:
 *  - Subscription-driven: a patient's stream starts when the first handler for `ecg:{id}`, `vitals:{id}` or
 *    `riskStatus:{id}` is registered and stops when the last one is removed. That covers both the patient's own
 *    dashboard and a doctor opening a patient, without modelling hub groups.
 *  - Only live devices stream: a patient who never paired a device (or whose device is "offline") produces
 *    nothing, so the dashboard shows its honest "awaiting live signal" state.
 *  - ECG samples are produced at exactly 250 Hz by counting how many samples became due since the last tick, so
 *    the rate stays correct even if the browser delays timers; a long stall skips the backlog instead of replaying it.
 *  - Patients never receive the risk score / confidence / flags, matching the REST rule.
 *  - Scheduler, clock and patient lookup are injectable, which keeps the class unit-testable without real time.
 * Implementation: a class exposing the subset of the HubConnection API the app uses (on, off, invoke, start,
 *    stop, onreconnecting, onreconnected, onclose, state).
 */
import {
  DEMO_ECG_SAMPLE_RATE_HZ,
  DEMO_RISK_TICK_MS,
  DEMO_VITALS_TICK_MS,
} from '@/shared/demo/demoConstants';
import { seedFromString } from '@/shared/demo/demoMath';
import { ecgParamsFor, isDeviceLive, riskAt, vitalsAt } from '@/shared/demo/demoReadings';
import { loadDemoDb } from '@/shared/demo/demoStorage';
import { EcgGenerator } from '@/shared/demo/ecgSynth';
import type { DemoPatientRecord } from '@/shared/demo/demoTypes';
import type { RiskStatusPayload, UserRole, VitalsTickPayload } from '@/shared/types/api';
import { decodeJwtPayload } from '@/shared/utils/jwt';

/** How often the internal ticker runs; 25 ticks per second, ~10 ECG samples each. */
const TICK_INTERVAL_MS = 40;

/** Upper bound of ECG samples emitted by one tick (2 seconds' worth), so a stalled tab cannot flood the UI. */
const MAX_SAMPLES_PER_TICK = DEMO_ECG_SAMPLE_RATE_HZ * 2;

/** Matches the three per-patient event families the app subscribes to. */
const STREAM_EVENT = /^(?:ecg|vitals|riskStatus):(.+)$/;

type StoredHandler = (payload: unknown) => void;
type AnyHandler = (...args: never[]) => void;

/** Everything that touches time or storage, replaceable in tests. */
export interface DemoHubDeps {
  now: () => number;
  setInterval: (callback: () => void, ms: number) => unknown;
  clearInterval: (handle: unknown) => void;
  getPatient: (patientId: string) => DemoPatientRecord | null;
}

const DEFAULT_DEPS: DemoHubDeps = {
  now: () => Date.now(),
  setInterval: (callback, ms) => globalThis.setInterval(callback, ms),
  clearInterval: (handle) => globalThis.clearInterval(handle as ReturnType<typeof setInterval>),
  getPatient: (patientId) =>
    loadDemoDb().patients.find((patient) => patient.id === patientId) ?? null,
};

/** Per-patient simulation state. */
interface PatientStream {
  patient: DemoPatientRecord;
  generator: EcgGenerator;
  lastEcgMs: number;
  lastVitalsMs: number;
  lastRiskMs: number;
}

export class DemoHubConnection {
  private readonly handlers = new Map<string, Set<StoredHandler>>();
  private readonly streams = new Map<string, PatientStream>();
  private readonly deps: DemoHubDeps;
  private readonly accessTokenFactory: () => string | null;
  private ticker: unknown = null;
  private started = false;
  private generation = 0;
  private viewerRole: UserRole | null = null;

  /** `accessTokenFactory` is only used to learn the viewer's role (to redact doctor-only fields). */
  constructor(accessTokenFactory: () => string | null, deps: Partial<DemoHubDeps> = {}) {
    this.accessTokenFactory = accessTokenFactory;
    this.deps = { ...DEFAULT_DEPS, ...deps };
  }

  /** Mirrors HubConnection.state for the few callers that might read it. */
  get state(): string {
    return this.started ? 'Connected' : 'Disconnected';
  }

  /** Opens the (simulated) connection and begins any streams that handlers already asked for. */
  async start(): Promise<void> {
    this.generation += 1;
    const generation = this.generation;
    await Promise.resolve(); // one async hop, like a real handshake

    // stop() (or a second start()) ran while we were "connecting": this attempt is obsolete.
    if (generation !== this.generation) return;

    const token = this.accessTokenFactory();
    this.viewerRole = token ? (decodeJwtPayload(token)?.role ?? null) : null;
    this.started = true;
    this.syncStreams();
  }

  /** Closes the connection and every stream. Unlike a real hub it does not fire onclose callbacks. */
  async stop(): Promise<void> {
    this.generation += 1;
    this.started = false;
    this.syncStreams();
  }

  /** Registers a handler for a server-to-client event such as `ecg:{patientId}`. */
  on(eventName: string, handler: AnyHandler): void {
    const set = this.handlers.get(eventName) ?? new Set<StoredHandler>();
    set.add(handler as StoredHandler);
    this.handlers.set(eventName, set);
    this.syncStreams();
  }

  /** Removes one handler (or every handler of the event when none is given). */
  off(eventName: string, handler?: AnyHandler): void {
    if (handler) {
      this.handlers.get(eventName)?.delete(handler as StoredHandler);
    } else {
      this.handlers.delete(eventName);
    }
    this.syncStreams();
  }

  /** Client-to-server calls. Group membership is implicit in the demo, so joining/leaving is a no-op. */
  invoke(methodName: string): Promise<unknown> {
    if (methodName === 'JoinPatientGroup' || methodName === 'LeavePatientGroup') {
      return Promise.resolve(undefined);
    }
    return Promise.reject(new Error(`The demo hub has no method "${methodName}".`));
  }

  /** The demo connection cannot drop, so reconnection callbacks are accepted and never fired. */
  onreconnecting(): void {
    // Intentionally empty.
  }

  /** See onreconnecting. */
  onreconnected(): void {
    // Intentionally empty.
  }

  /** See onreconnecting. */
  onclose(): void {
    // Intentionally empty.
  }

  /** Starts / stops per-patient streams so they match the current set of subscriptions. */
  private syncStreams(): void {
    const wanted = new Set<string>();
    if (this.started) {
      for (const [eventName, set] of this.handlers) {
        if (set.size === 0) continue;
        const patientId = STREAM_EVENT.exec(eventName)?.[1];
        if (patientId) wanted.add(patientId);
      }
    }

    for (const patientId of [...this.streams.keys()]) {
      if (!wanted.has(patientId)) this.streams.delete(patientId);
    }
    for (const patientId of wanted) {
      if (this.streams.has(patientId)) continue;
      const patient = this.deps.getPatient(patientId);
      // Patients without a live device stream nothing: the UI then shows its honest "awaiting signal" state.
      if (patient && isDeviceLive(patient)) this.streams.set(patientId, this.createStream(patient));
    }

    if (this.streams.size > 0 && this.ticker === null) {
      this.ticker = this.deps.setInterval(() => this.tick(), TICK_INTERVAL_MS);
    } else if (this.streams.size === 0 && this.ticker !== null) {
      this.deps.clearInterval(this.ticker);
      this.ticker = null;
    }
  }

  /** Creates the simulation state for a patient, starting "now". */
  private createStream(patient: DemoPatientRecord): PatientStream {
    const nowMs = this.deps.now();
    const heartRate = vitalsAt(patient, nowMs).heartRate;
    return {
      patient,
      generator: new EcgGenerator(ecgParamsFor(patient, heartRate), seedFromString(patient.id)),
      lastEcgMs: nowMs,
      lastVitalsMs: nowMs,
      lastRiskMs: nowMs,
    };
  }

  /** One scheduler beat: emit whatever ECG samples, vitals tick and risk update have become due. */
  private tick(): void {
    const nowMs = this.deps.now();
    for (const [patientId, stream] of this.streams) {
      this.emitDueEcgSamples(patientId, stream, nowMs);

      if (nowMs - stream.lastVitalsMs >= DEMO_VITALS_TICK_MS) {
        this.emitVitals(patientId, stream, nowMs);
      }
      if (nowMs - stream.lastRiskMs >= DEMO_RISK_TICK_MS) {
        this.emitRisk(patientId, stream, nowMs);
      }
    }
  }

  /** Emits exactly the ECG samples that became due since the last tick. */
  private emitDueEcgSamples(patientId: string, stream: PatientStream, nowMs: number): void {
    const due = Math.min(
      MAX_SAMPLES_PER_TICK,
      Math.floor(((nowMs - stream.lastEcgMs) * DEMO_ECG_SAMPLE_RATE_HZ) / 1000),
    );
    if (due <= 0) return;

    const eventName = `ecg:${patientId}`;
    for (let i = 0; i < due; i += 1) this.emit(eventName, stream.generator.next());
    stream.lastEcgMs += (due * 1000) / DEMO_ECG_SAMPLE_RATE_HZ;
    // After a long stall (throttled background tab) skip the backlog instead of replaying it later.
    if (nowMs - stream.lastEcgMs > 1000) stream.lastEcgMs = nowMs;
  }

  /** Heart-rate / SpO2 tick; also re-tunes the ECG so the trace speed follows the displayed heart rate. */
  private emitVitals(patientId: string, stream: PatientStream, nowMs: number): void {
    const vitals = vitalsAt(stream.patient, nowMs);
    stream.generator.setParams(ecgParamsFor(stream.patient, vitals.heartRate));
    stream.lastVitalsMs = nowMs;

    const payload: VitalsTickPayload = {
      patientId,
      heartRate: vitals.heartRate,
      spo2: vitals.spo2,
      timestamp: new Date(nowMs).toISOString(),
    };
    this.emit(`vitals:${patientId}`, payload);
  }

  /** Risk-status push; patients get the status only (no score, confidence or flags). */
  private emitRisk(patientId: string, stream: PatientStream, nowMs: number): void {
    const risk = riskAt(stream.patient, nowMs);
    stream.lastRiskMs = nowMs;
    const timestamp = new Date(nowMs).toISOString();

    const payload: RiskStatusPayload =
      this.viewerRole === 'Patient'
        ? { patientId, statusLevel: risk.statusLevel, confidenceScore: 0, flags: [], timestamp }
        : {
            patientId,
            statusLevel: risk.statusLevel,
            confidenceScore: risk.confidenceScore,
            riskScore: risk.riskScore,
            flags: risk.flags,
            timestamp,
          };
    this.emit(`riskStatus:${patientId}`, payload);
  }

  /** Calls every handler of an event; one failing handler never stops the stream. */
  private emit(eventName: string, payload: unknown): void {
    const set = this.handlers.get(eventName);
    if (!set) return;
    for (const handler of [...set]) {
      try {
        handler(payload);
      } catch (error) {
        console.error(`Demo hub handler for "${eventName}" threw:`, error);
      }
    }
  }
}

/** Factory used by shared/lib/signalrClient.ts. */
export function createDemoHubConnection(
  accessTokenFactory: () => string | null,
  deps?: Partial<DemoHubDeps>,
): DemoHubConnection {
  return new DemoHubConnection(accessTokenFactory, deps);
}
