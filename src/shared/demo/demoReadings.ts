/**
 * File: src/shared/demo/demoReadings.ts
 * Layer: shared/demo (in-browser demo backend)
 * Purpose: Pure functions that turn a patient's stored baseline plus a moment in time into the numbers the
 *          dashboards show: vitals, risk assessment, ECG intervals, electrode quality, the hourly timeline and
 *          the vitals history series.
 * Logic:
 *  - Nothing time-varying is stored. Every value is a deterministic function of (patient, time), so a REST
 *    request and a hub push for the same instant agree, and history can be generated for any range for free.
 *  - Scenario (stable / caution / critical) selects the baseline spread, ECG morphology and timeline pattern.
 *  - A patient with an offline device reports "as of" an earlier moment; a patient with no device has no readings.
 * Implementation: small helpers over demoMath.ts (hash, smooth noise); no I/O, no clock access (callers pass `nowMs`).
 */
import type { RiskStatusLevel } from '@/shared/types/api';
import { clamp, hash01, roundTo, seedFromString, valueNoise } from '@/shared/demo/demoMath';
import type { DemoPatientRecord, DemoScenario } from '@/shared/demo/demoTypes';
import type { EcgParams } from '@/shared/demo/ecgSynth';

const MINUTE_MS = 60_000;
const HOUR_MS = 3_600_000;
const DAY_MS = 86_400_000;

type ActiveScenario = Exclude<DemoScenario, 'idle'>;

/** Exact strings the real backend sends for the electrode check (see ElectrodePlacementStatus mapping). */
export const PLACEMENT_OPTIMAL = 'Optimal Placement';
export const PLACEMENT_REPOSITION = 'Reposition Needed';

/** Stable numeric seed for a patient; all noise streams are offsets of this value. */
export function patientSeed(patient: DemoPatientRecord): number {
  return seedFromString(patient.id);
}

/** True once the patient has a device sending data (idle = never paired). */
export function hasReadings(patient: DemoPatientRecord): boolean {
  return patient.scenario !== 'idle';
}

/** True when the device is transmitting right now (readings exist and it has not gone offline). */
export function isDeviceLive(patient: DemoPatientRecord): boolean {
  return hasReadings(patient) && patient.offlineMinutes === 0;
}

/** The moment the patient's data is "as of": now, or earlier for a device that stopped transmitting. */
export function dataTimeMs(patient: DemoPatientRecord, nowMs: number): number {
  return nowMs - patient.offlineMinutes * MINUTE_MS;
}

/** Risk level implied by the scenario, or null when there are no readings. */
export function statusLevelOf(patient: DemoPatientRecord): RiskStatusLevel | null {
  switch (patient.scenario) {
    case 'stable':
      return 'Stable';
    case 'caution':
      return 'Caution';
    case 'critical':
      return 'Critical';
    default:
      return null;
  }
}

/** Scenario used for numeric spreads; an idle patient never reaches these functions in practice. */
function activeScenario(patient: DemoPatientRecord): ActiveScenario {
  return patient.scenario === 'idle' ? 'stable' : patient.scenario;
}

// ---------------------------------------------------------------------------
// Vitals
// ---------------------------------------------------------------------------

export interface VitalsReading {
  heartRate: number;
  spo2: number;
}

const HR_SPREAD: Record<ActiveScenario, number> = { stable: 3, caution: 6, critical: 5 };
const SPO2_SPREAD: Record<ActiveScenario, number> = { stable: 1, caution: 1.5, critical: 1.5 };

/**
 * Heart rate and SpO2 at an instant: baseline + a day/night rhythm + slow drift + smooth and per-2-second noise.
 * Integer outputs, like a real pulse oximeter.
 */
export function vitalsAt(patient: DemoPatientRecord, tMs: number): VitalsReading {
  const scenario = activeScenario(patient);
  const seed = patientSeed(patient);
  const date = new Date(tMs);
  const hourOfDay = date.getHours() + date.getMinutes() / 60;
  // -1 around 04:00 (resting), +1 around 16:00 (most active).
  const circadian = -Math.cos(((hourOfDay - 4) / 24) * 2 * Math.PI);

  const slow = valueNoise(seed, tMs, 45_000) - 0.5;
  const drift = valueNoise(seed + 7, tMs, 36 * HOUR_MS) - 0.5;
  const fast = hash01(seed + 3, Math.floor(tMs / 2000)) - 0.5;
  const heartRate =
    patient.baseline.heartRate +
    circadian * 2.5 +
    slow * 2 * HR_SPREAD[scenario] +
    drift * 4 +
    fast * 3;

  const oxygenNoise = valueNoise(seed + 11, tMs, 120_000) - 0.5;
  const spo2 = patient.baseline.spo2 + oxygenNoise * 2 * SPO2_SPREAD[scenario];

  return {
    heartRate: Math.round(clamp(heartRate, 35, 190)),
    spo2: Math.round(clamp(spo2, 80, 100)),
  };
}

/** Min / max heart rate over the last 24 hours (the "Min: 62 - Max: 98" line). */
export function heartRateRange24h(
  patient: DemoPatientRecord,
  nowMs: number,
): { min: number; max: number } {
  const series = historySeries(patient, '24h', nowMs);
  if (series.length === 0) return { min: 0, max: 0 };
  let min = Infinity;
  let max = -Infinity;
  for (const point of series) {
    if (point.heartRate < min) min = point.heartRate;
    if (point.heartRate > max) max = point.heartRate;
  }
  return { min, max };
}

// ---------------------------------------------------------------------------
// Risk assessment
// ---------------------------------------------------------------------------

export interface RiskReading {
  statusLevel: RiskStatusLevel;
  riskScore: number;
  confidenceScore: number;
  flags: string[];
}

/** Doctor-only model flags implied by the scenario. */
function flagsFor(patient: DemoPatientRecord): string[] {
  if (patient.scenario === 'critical') return ['possible_st_elevation'];
  if (patient.scenario === 'caution') {
    return patient.alertKind === 'Technical' ? ['poor_electrode_contact'] : ['irregular_rhythm'];
  }
  return [];
}

/** Risk level, 0-100 score (jittering a few points around the baseline), confidence and flags. */
export function riskAt(patient: DemoPatientRecord, tMs: number): RiskReading {
  const seed = patientSeed(patient) + 101;
  const scoreJitter = (valueNoise(seed, tMs, 5 * MINUTE_MS) - 0.5) * 6;
  const confidenceJitter = (valueNoise(seed + 1, tMs, 5 * MINUTE_MS) - 0.5) * 0.04;
  return {
    statusLevel: statusLevelOf(patient) ?? 'Stable',
    riskScore: clamp(Math.round(patient.baseline.riskScore + scoreJitter), 0, 100),
    confidenceScore: roundTo(clamp(patient.baseline.confidence + confidenceJitter, 0.5, 0.99), 2),
    flags: flagsFor(patient),
  };
}

// ---------------------------------------------------------------------------
// ECG intervals, waveform parameters and electrode quality
// ---------------------------------------------------------------------------

export interface EcgFeatureReading {
  prIntervalMs: number;
  qrsDurationMs: number;
  qtMs: number;
  qtcMs: number;
}

const ECG_FEATURES: Record<ActiveScenario, EcgFeatureReading> = {
  stable: { prIntervalMs: 152, qrsDurationMs: 88, qtMs: 396, qtcMs: 418 },
  caution: { prIntervalMs: 164, qrsDurationMs: 94, qtMs: 404, qtcMs: 441 },
  critical: { prIntervalMs: 172, qrsDurationMs: 126, qtMs: 436, qtcMs: 498 },
};

/** PR / QRS / QT / QTc for the scenario with a few milliseconds of natural variation. */
export function ecgFeaturesAt(patient: DemoPatientRecord, tMs: number): EcgFeatureReading {
  const base = ECG_FEATURES[activeScenario(patient)];
  const seed = patientSeed(patient) + 21;
  const wobble = (offset: number) => (valueNoise(seed + offset, tMs, 60_000) - 0.5) * 8;
  return {
    prIntervalMs: Math.round(base.prIntervalMs + wobble(0)),
    qrsDurationMs: Math.round(base.qrsDurationMs + wobble(1)),
    qtMs: Math.round(base.qtMs + wobble(2)),
    qtcMs: Math.round(base.qtcMs + wobble(3)),
  };
}

/** Knobs for the ECG synthesizer: morphology follows the scenario, rate follows the current heart rate. */
export function ecgParamsFor(patient: DemoPatientRecord, heartRate: number): EcgParams {
  switch (patient.scenario) {
    case 'critical':
      return {
        heartRate,
        stElevation: 0.35,
        qrsWidthScale: 1.35,
        noise: 0.02,
        irregularity: 0.05,
        ectopicChance: 0.06,
      };
    case 'caution':
      return patient.alertKind === 'Technical'
        ? {
            heartRate,
            stElevation: 0,
            qrsWidthScale: 1,
            noise: 0.07,
            irregularity: 0.03,
            ectopicChance: 0,
          }
        : {
            heartRate,
            stElevation: 0.02,
            qrsWidthScale: 1.05,
            noise: 0.02,
            irregularity: 0.08,
            ectopicChance: 0.1,
          };
    default:
      return {
        heartRate,
        stElevation: 0,
        qrsWidthScale: 1,
        noise: 0.015,
        irregularity: 0.03,
        ectopicChance: 0,
      };
  }
}

export interface ElectrodeReading {
  placementStatus: string;
  confidenceScore: number;
  impedanceKOhm: Record<string, number>;
}

/** Electrode-placement check result; the "technical alert" patient reports poor contact. */
export function electrodeAt(patient: DemoPatientRecord, tMs: number): ElectrodeReading {
  const seed = patientSeed(patient) + 41;
  const technical = patient.alertKind === 'Technical';
  const wobble = (offset: number) => valueNoise(seed + offset, tMs, 120_000);
  const baseConfidence = technical ? 0.62 : patient.scenario === 'critical' ? 0.9 : 0.96;
  const baseImpedance = technical ? 4.6 : 1.7;
  return {
    placementStatus: technical ? PLACEMENT_REPOSITION : PLACEMENT_OPTIMAL,
    confidenceScore: roundTo(clamp(baseConfidence + (wobble(0) - 0.5) * 0.04, 0.5, 0.99), 2),
    impedanceKOhm: {
      RA: roundTo(baseImpedance + wobble(1) * 0.5, 1),
      LA: roundTo(baseImpedance + wobble(2) * 0.5, 1),
      LL: roundTo(baseImpedance + wobble(3) * 0.5, 1),
    },
  };
}

// ---------------------------------------------------------------------------
// Hourly timeline and history
// ---------------------------------------------------------------------------

export interface TimelineBucket {
  hour: number;
  status: RiskStatusLevel;
  isCurrent: boolean;
}

/** Status of one past hour; the current hour always equals the live status. */
function statusForHour(
  patient: DemoPatientRecord,
  hourStartMs: number,
  hoursAgo: number,
): RiskStatusLevel {
  const live = statusLevelOf(patient) ?? 'Stable';
  if (hoursAgo === 0) return live;
  const roll = hash01(patientSeed(patient) + 31, Math.floor(hourStartMs / HOUR_MS));

  switch (patient.scenario) {
    case 'caution':
      if (hoursAgo <= 5) return roll < 0.8 ? 'Caution' : 'Stable';
      return roll < 0.2 ? 'Caution' : 'Stable';
    case 'critical':
      if (hoursAgo <= 2) return 'Critical';
      if (hoursAgo <= 7) return roll < 0.6 ? 'Caution' : 'Critical';
      return roll < 0.25 ? 'Caution' : 'Stable';
    default:
      return roll < 0.06 ? 'Caution' : 'Stable';
  }
}

/** Up to 24 hourly buckets, oldest first, ending with the current hour. Empty when there are no readings. */
export function hourlyTimeline(patient: DemoPatientRecord, nowMs: number): TimelineBucket[] {
  if (!hasReadings(patient)) return [];

  const currentHourStart = Math.floor(dataTimeMs(patient, nowMs) / HOUR_MS) * HOUR_MS;
  const firstHourStart =
    patient.dataSinceMs === null ? -Infinity : Math.floor(patient.dataSinceMs / HOUR_MS) * HOUR_MS;
  const hoursBack = clamp(Math.floor((currentHourStart - firstHourStart) / HOUR_MS), 0, 23);

  const buckets: TimelineBucket[] = [];
  for (let hoursAgo = hoursBack; hoursAgo >= 0; hoursAgo -= 1) {
    const hourStart = currentHourStart - hoursAgo * HOUR_MS;
    buckets.push({
      hour: new Date(hourStart).getHours(),
      status: statusForHour(patient, hourStart, hoursAgo),
      isCurrent: hoursAgo === 0,
    });
  }
  return buckets;
}

export type HistoryRangeKey = '24h' | '7d' | '30d';

const HISTORY_CONFIG: Record<HistoryRangeKey, { spanMs: number; stepMs: number }> = {
  '24h': { spanMs: DAY_MS, stepMs: 5 * MINUTE_MS },
  '7d': { spanMs: 7 * DAY_MS, stepMs: HOUR_MS },
  '30d': { spanMs: 30 * DAY_MS, stepMs: 6 * HOUR_MS },
};

/** Type guard for the `?range=` query parameter. */
export function isHistoryRange(value: string): value is HistoryRangeKey {
  return Object.prototype.hasOwnProperty.call(HISTORY_CONFIG, value);
}

export interface HistoryPoint {
  timestampMs: number;
  heartRate: number;
  spo2: number;
}

/** One point per time bucket (5 min / 1 h / 6 h), oldest first; skips time before the device was paired. */
export function historySeries(
  patient: DemoPatientRecord,
  range: HistoryRangeKey,
  nowMs: number,
): HistoryPoint[] {
  if (!hasReadings(patient)) return [];

  const { spanMs, stepMs } = HISTORY_CONFIG[range];
  const end = Math.floor(dataTimeMs(patient, nowMs) / stepMs) * stepMs;
  const start = end - spanMs + stepMs;
  const earliest = patient.dataSinceMs ?? -Infinity;

  const points: HistoryPoint[] = [];
  for (let t = start; t <= end; t += stepMs) {
    if (t < earliest) continue;
    const reading = vitalsAt(patient, t);
    points.push({ timestampMs: t, heartRate: reading.heartRate, spo2: reading.spo2 });
  }
  return points;
}

/** Length of a history range in milliseconds (used to count alerts per report period). */
export function historySpanMs(range: HistoryRangeKey): number {
  return HISTORY_CONFIG[range].spanMs;
}
