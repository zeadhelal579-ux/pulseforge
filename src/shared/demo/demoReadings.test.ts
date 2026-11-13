import { describe, expect, it } from 'vitest';
import { createSeedDatabase } from '@/shared/demo/demoSeed';
import {
  dataTimeMs,
  ecgFeaturesAt,
  electrodeAt,
  hasReadings,
  heartRateRange24h,
  historySeries,
  hourlyTimeline,
  isDeviceLive,
  riskAt,
  vitalsAt,
  PLACEMENT_REPOSITION,
} from '@/shared/demo/demoReadings';
import type { DemoPatientRecord } from '@/shared/demo/demoTypes';
import { FIXED_NOW } from '@/test/demoTestUtils';

const patients = createSeedDatabase(FIXED_NOW).patients;
const byId = (id: string): DemoPatientRecord => {
  const patient = patients.find((candidate) => candidate.id === id);
  if (!patient) throw new Error(`missing seeded patient ${id}`);
  return patient;
};
const stable = byId('demo-patient-1');
const critical = byId('demo-patient-3');
const offline = byId('demo-patient-8');
const technical = byId('demo-patient-10');

describe('demoReadings', () => {
  it('returns the same value for the same patient and instant, and moves over time', () => {
    expect(vitalsAt(stable, FIXED_NOW)).toEqual(vitalsAt(stable, FIXED_NOW));
    const values = new Set<number>();
    for (let minute = 0; minute < 60; minute += 1) values.add(vitalsAt(stable, FIXED_NOW + minute * 60_000).heartRate);
    expect(values.size).toBeGreaterThan(3);
  });

  it('keeps each scenario inside a clinically sensible band', () => {
    for (let hour = 0; hour < 48; hour += 1) {
      const t = FIXED_NOW + hour * 3_600_000;
      const calm = vitalsAt(stable, t);
      const sick = vitalsAt(critical, t);
      expect(calm.heartRate).toBeGreaterThan(55);
      expect(calm.heartRate).toBeLessThan(95);
      expect(calm.spo2).toBeGreaterThanOrEqual(95);
      expect(sick.heartRate).toBeGreaterThan(100);
      expect(sick.spo2).toBeLessThanOrEqual(94);
      expect(riskAt(stable, t).riskScore).toBeLessThan(35);
      expect(riskAt(critical, t).riskScore).toBeGreaterThan(70);
    }
  });

  it('reports flags and wider intervals for the critical scenario only', () => {
    expect(riskAt(critical, FIXED_NOW).flags).toContain('possible_st_elevation');
    expect(riskAt(stable, FIXED_NOW).flags).toEqual([]);
    expect(ecgFeaturesAt(critical, FIXED_NOW).qrsDurationMs).toBeGreaterThan(110);
    expect(ecgFeaturesAt(stable, FIXED_NOW).qrsDurationMs).toBeLessThan(100);
  });

  it('reports weak electrode contact for the technical-alert patient', () => {
    expect(electrodeAt(technical, FIXED_NOW).placementStatus).toBe(PLACEMENT_REPOSITION);
    expect(electrodeAt(technical, FIXED_NOW).confidenceScore).toBeLessThan(0.7);
    expect(electrodeAt(stable, FIXED_NOW).confidenceScore).toBeGreaterThan(0.9);
  });

  it('shifts an offline device back in time and marks it not live', () => {
    expect(isDeviceLive(offline)).toBe(false);
    expect(isDeviceLive(stable)).toBe(true);
    expect(FIXED_NOW - dataTimeMs(offline, FIXED_NOW)).toBe(38 * 60_000);
  });

  it('builds a 24-hour timeline ending with the current status', () => {
    const timeline = hourlyTimeline(critical, FIXED_NOW);
    expect(timeline).toHaveLength(24);
    expect(timeline[23]).toMatchObject({ status: 'Critical', isCurrent: true });
    expect(timeline.slice(0, 23).every((bucket) => !bucket.isCurrent)).toBe(true);
    // The last few hours of a critical patient are not "Stable".
    expect(timeline.slice(-3).every((bucket) => bucket.status === 'Critical')).toBe(true);
  });

  it('only covers the period since pairing for a freshly paired device', () => {
    const fresh: DemoPatientRecord = { ...stable, dataSinceMs: FIXED_NOW - 3 * 3_600_000 - 5 * 60_000 };
    // Data exists since 08:55 UTC, so the hourly buckets are 08, 09, 10, 11 and the current 12 o'clock hour.
    expect(hourlyTimeline(fresh, FIXED_NOW)).toHaveLength(5);
    expect(historySeries(fresh, '24h', FIXED_NOW).length).toBeLessThan(50);
    expect(historySeries(fresh, '24h', FIXED_NOW).length).toBeGreaterThan(30);
  });

  it('has no readings at all for a patient without a device', () => {
    const idle: DemoPatientRecord = { ...stable, scenario: 'idle' };
    expect(hasReadings(idle)).toBe(false);
    expect(hourlyTimeline(idle, FIXED_NOW)).toEqual([]);
    expect(historySeries(idle, '30d', FIXED_NOW)).toEqual([]);
    expect(heartRateRange24h(idle, FIXED_NOW)).toEqual({ min: 0, max: 0 });
  });

  it('computes a min/max range that brackets the current heart rate', () => {
    const range = heartRateRange24h(stable, FIXED_NOW);
    expect(range.min).toBeLessThanOrEqual(range.max);
    const now = vitalsAt(stable, FIXED_NOW).heartRate;
    expect(now).toBeGreaterThanOrEqual(range.min - 6);
    expect(now).toBeLessThanOrEqual(range.max + 6);
  });
});
