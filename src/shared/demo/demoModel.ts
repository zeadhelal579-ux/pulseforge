/**
 * File: src/shared/demo/demoModel.ts
 * Layer: shared/demo (in-browser demo backend)
 * Purpose: Assembles the exact DTOs the frontend expects (PatientProfile, PatientListItem, WardStats, Alert[],
 *          AiReport, VitalsSample[], ...) from stored records plus the derived readings.
 * Logic:
 *  - Mirrors the real backend's product rules: a patient viewer never receives the numeric risk score,
 *    model confidence or model flags (they are stripped here, not just hidden by the UI), while the doctor does.
 *  - Seeded alerts are generated relative to "now" so a demo opened days later still looks alive; alerts created
 *    at runtime (manual events) are stored and merged in.
 *  - Server-generated text is localised per request (see demoText.ts).
 * Implementation: pure builder functions over DemoDatabase; they never write to storage.
 */
import { DEMO_CRITICAL_CAPACITY, DEMO_WARD_CAPACITY } from '@/shared/demo/demoConstants';
import { roundTo } from '@/shared/demo/demoMath';
import {
  dataTimeMs,
  ecgFeaturesAt,
  electrodeAt,
  hasReadings,
  heartRateRange24h,
  historySeries,
  historySpanMs,
  hourlyTimeline,
  isDeviceLive,
  riskAt,
  statusLevelOf,
  vitalsAt,
  type HistoryRangeKey,
} from '@/shared/demo/demoReadings';
import {
  ALERT_TYPE_RISK,
  SEED_ALERT_TEXT,
  SEED_ESCALATION_NOTE,
  SEED_RESOLUTION_NOTE,
  alertTypeText,
  buildNarrative,
  pick,
  statusLabelText,
  type DemoLanguage,
  type SeedAlertKey,
  type StatusLabelKey,
} from '@/shared/demo/demoText';
import type {
  DemoDatabase,
  DemoDeviceRecord,
  DemoDoctorRecord,
  DemoPatientRecord,
  DemoUserRecord,
} from '@/shared/demo/demoTypes';
import type {
  AiReport,
  AiReportPeriod,
  Alert,
  AlertSeverity,
  AuthenticatedUser,
  DeviceSummary,
  DoctorProfile,
  PatientListItem,
  PatientProfile,
  UserRole,
  VitalsSample,
  WardStats,
} from '@/shared/types/api';

const MINUTE_MS = 60_000;

/** Model version string reported with every simulated assessment. */
export const DEMO_MODEL_VERSION = 'demo-v0.1 (simulated)';

/** Epoch ms -> ISO timestamp. */
function iso(ms: number): string {
  return new Date(ms).toISOString();
}

// ---------------------------------------------------------------------------
// Relationship lookups
// ---------------------------------------------------------------------------

/** True when the doctor is linked to the patient (the basis of every doctor-side access check). */
export function isAssigned(db: DemoDatabase, doctorId: string, patientId: string): boolean {
  return db.assignments.some((a) => a.doctorId === doctorId && a.patientId === patientId);
}

/** The patients linked to one doctor. */
export function patientsOfDoctor(db: DemoDatabase, doctorId: string): DemoPatientRecord[] {
  const ids = new Set(
    db.assignments.filter((a) => a.doctorId === doctorId).map((a) => a.patientId),
  );
  return db.patients.filter((patient) => ids.has(patient.id));
}

/** The doctor the "Call" button rings: the primary assignment, else the first one. */
export function primaryDoctorOf(db: DemoDatabase, patientId: string): DemoDoctorRecord | null {
  const assignments = db.assignments.filter((a) => a.patientId === patientId);
  const chosen = assignments.find((a) => a.isPrimary) ?? assignments[0];
  if (!chosen) return null;
  return db.doctors.find((doctor) => doctor.id === chosen.doctorId) ?? null;
}

/** The device paired to a patient, if any. */
export function deviceOf(db: DemoDatabase, patientId: string): DemoDeviceRecord | null {
  return db.devices.find((device) => device.patientId === patientId) ?? null;
}

// ---------------------------------------------------------------------------
// Account and profile DTOs
// ---------------------------------------------------------------------------

/** Session user returned by login / signup. */
export function buildAuthenticatedUser(db: DemoDatabase, user: DemoUserRecord): AuthenticatedUser {
  if (user.role === 'Doctor') {
    const doctor = db.doctors.find((candidate) => candidate.id === user.profileId);
    return {
      id: user.id,
      role: 'Doctor',
      fullName: doctor?.fullName ?? user.email,
      email: user.email,
      preferredLanguage: user.preferredLanguage,
      doctorId: user.profileId,
      specialty: doctor?.specialty,
    };
  }
  const patient = db.patients.find((candidate) => candidate.id === user.profileId);
  return {
    id: user.id,
    role: 'Patient',
    fullName: patient?.fullName ?? user.email,
    email: user.email,
    preferredLanguage: user.preferredLanguage,
    patientId: user.profileId,
  };
}

/** `GET /doctors/me`. */
export function buildDoctorProfile(doctor: DemoDoctorRecord, user: DemoUserRecord): DoctorProfile {
  return {
    id: doctor.id,
    fullName: doctor.fullName,
    specialty: doctor.specialty,
    licenseNumber: doctor.licenseNumber,
    hospital: doctor.hospital,
    department: doctor.department,
    phone: doctor.phone,
    email: doctor.email,
    preferredLanguage: user.preferredLanguage,
  };
}

/** Device card data (battery, connection, signal). A live device was "seen" at the data time. */
export function buildDeviceSummary(
  device: DemoDeviceRecord,
  patient: DemoPatientRecord,
  nowMs: number,
): DeviceSummary {
  return {
    serialNumber: device.serialNumber,
    firmwareVersion: device.firmwareVersion,
    batteryLevel: device.batteryLevel,
    lastSeenAt: iso(dataTimeMs(patient, nowMs)),
    connectionMode: device.connectionMode,
    signalStrengthDbm: device.signalStrengthDbm,
  };
}

// ---------------------------------------------------------------------------
// Alerts
// ---------------------------------------------------------------------------

interface SeedAlertSpec {
  key: SeedAlertKey;
  minutesAgo: number;
  severity: AlertSeverity;
  resolution: 'auto' | 'escalated' | null;
}

/** The scripted alert history of each scenario (newest first; offsets are relative to "now"). */
function seedAlertSpecs(patient: DemoPatientRecord): SeedAlertSpec[] {
  switch (patient.scenario) {
    case 'caution':
      return patient.alertKind === 'Technical'
        ? [
            { key: 'signalQuality', minutesAgo: 22, severity: 'Moderate', resolution: null },
            { key: 'briefRise', minutesAgo: 900, severity: 'Transient', resolution: 'auto' },
          ]
        : [
            { key: 'irregularRhythm', minutesAgo: 14, severity: 'Moderate', resolution: null },
            { key: 'briefRise', minutesAgo: 380, severity: 'Transient', resolution: 'auto' },
            { key: 'activityRise', minutesAgo: 1500, severity: 'Info', resolution: 'auto' },
          ];
    case 'critical':
      return [
        { key: 'criticalRaised', minutesAgo: 6, severity: 'Critical', resolution: null },
        { key: 'escalated', minutesAgo: 48, severity: 'Moderate', resolution: 'escalated' },
        { key: 'briefRise', minutesAgo: 600, severity: 'Transient', resolution: 'auto' },
      ];
    case 'stable':
      return [{ key: 'activityRise', minutesAgo: 1350, severity: 'Info', resolution: 'auto' }];
    default:
      return [];
  }
}

/** Alert timeline: scripted history plus manual events, newest first, capped at 100 like the real endpoint. */
export function buildAlerts(
  db: DemoDatabase,
  patient: DemoPatientRecord,
  nowMs: number,
  language: DemoLanguage,
): Alert[] {
  const seeded = seedAlertSpecs(patient).map<Alert>((spec, index) => {
    const triggeredMs = nowMs - spec.minutesAgo * MINUTE_MS;
    let resolutionNote: string | null = null;
    if (spec.resolution === 'auto') resolutionNote = pick(SEED_RESOLUTION_NOTE, language);
    if (spec.resolution === 'escalated') resolutionNote = pick(SEED_ESCALATION_NOTE, language);

    return {
      id: `${patient.id}-seed-alert-${index + 1}`,
      patientId: patient.id,
      type: alertTypeText(ALERT_TYPE_RISK, language),
      severity: spec.severity,
      message: pick(SEED_ALERT_TEXT[spec.key], language),
      triggeredAt: iso(triggeredMs),
      resolvedAt: spec.resolution ? iso(triggeredMs + 12 * MINUTE_MS) : null,
      resolutionNote,
    };
  });

  const manual = db.manualAlerts
    .filter((alert) => alert.patientId === patient.id)
    .map<Alert>((alert) => ({ ...alert, type: alertTypeText(alert.type, language) }));

  return [...seeded, ...manual]
    .sort((a, b) => Date.parse(b.triggeredAt) - Date.parse(a.triggeredAt))
    .slice(0, 100);
}

// ---------------------------------------------------------------------------
// Patient aggregate
// ---------------------------------------------------------------------------

/**
 * `GET /patients/me` and `GET /patients/{id}`: the full aggregate the detail page renders from.
 * `viewer` decides whether doctor-only model output (score, confidence, flags) is included.
 */
export function buildPatientProfile(
  db: DemoDatabase,
  patient: DemoPatientRecord,
  viewer: UserRole,
  nowMs: number,
): PatientProfile {
  const user = db.users.find((candidate) => candidate.id === patient.userId);
  const doctor = primaryDoctorOf(db, patient.id);
  const device = deviceOf(db, patient.id);
  const readingsExist = hasReadings(patient);
  const t = dataTimeMs(patient, nowMs);
  const deviceId = device?.serialNumber ?? 'demo-device';

  let latestVitals: PatientProfile['latestVitals'] = null;
  let latestEcgFeatureSet: PatientProfile['latestEcgFeatureSet'] = null;
  let latestRiskAssessment: PatientProfile['latestRiskAssessment'] = null;
  let latestElectrodeVerification: PatientProfile['latestElectrodeVerification'] = null;

  if (readingsExist) {
    const vitals = vitalsAt(patient, t);
    const features = ecgFeaturesAt(patient, t);
    const risk = riskAt(patient, t);
    const electrode = electrodeAt(patient, t);
    const stamp = iso(t);

    latestVitals = {
      id: `${patient.id}-vitals-${Math.floor(t / 2000)}`,
      patientId: patient.id,
      deviceId,
      timestamp: stamp,
      heartRate: vitals.heartRate,
      spo2: vitals.spo2,
    };
    latestEcgFeatureSet = {
      id: `${patient.id}-ecg-${Math.floor(t / 60_000)}`,
      patientId: patient.id,
      timestamp: stamp,
      prIntervalMs: features.prIntervalMs,
      qrsDurationMs: features.qrsDurationMs,
      qtQtcMs: [features.qtMs, features.qtcMs],
    };
    // Product rule: patients only ever receive the STATUS. Score, confidence and flags are doctor-only.
    latestRiskAssessment =
      viewer === 'Doctor'
        ? {
            id: `${patient.id}-risk-${Math.floor(t / 300_000)}`,
            patientId: patient.id,
            timestamp: stamp,
            statusLevel: risk.statusLevel,
            confidenceScore: risk.confidenceScore,
            modelVersion: DEMO_MODEL_VERSION,
            flags: risk.flags,
            riskScore: risk.riskScore,
          }
        : {
            id: `${patient.id}-risk-${Math.floor(t / 300_000)}`,
            patientId: patient.id,
            timestamp: stamp,
            statusLevel: risk.statusLevel,
            confidenceScore: 0,
            modelVersion: DEMO_MODEL_VERSION,
            flags: [],
          };
    latestElectrodeVerification = {
      id: `${patient.id}-electrode-${Math.floor(t / 300_000)}`,
      patientId: patient.id,
      timestamp: stamp,
      placementStatus: electrode.placementStatus,
      confidenceScore: electrode.confidenceScore,
      impedanceKOhm: electrode.impedanceKOhm,
    };
  }

  return {
    id: patient.id,
    fullName: patient.fullName,
    age: patient.age,
    gender: patient.gender,
    nationalId: patient.nationalId,
    phone: patient.phone,
    email: patient.email,
    diagnosis: patient.diagnosis,
    medications: patient.medications,
    assignedDoctor: doctor
      ? {
          id: doctor.id,
          fullName: doctor.fullName,
          specialty: doctor.specialty,
          phone: doctor.phone,
        }
      : null,
    device: device ? buildDeviceSummary(device, patient, nowMs) : null,
    preferredLanguage: user?.preferredLanguage ?? 'ar',
    medicalHistory: patient.medicalHistory,
    latestVitals,
    latestEcgFeatureSet,
    latestRiskAssessment,
    latestElectrodeVerification,
    hourlyStatusTimeline: hourlyTimeline(patient, nowMs),
  };
}

// ---------------------------------------------------------------------------
// Doctor-side DTOs
// ---------------------------------------------------------------------------

/** Which localised status label a patient row shows. */
function statusLabelKey(patient: DemoPatientRecord): StatusLabelKey {
  if (!hasReadings(patient)) return 'awaiting';
  if (patient.scenario === 'critical') return 'critical';
  if (patient.scenario === 'caution') {
    return patient.alertKind === 'Technical' ? 'technical' : 'clinical';
  }
  return 'stable';
}

/** One row of the doctor's patient list. */
export function buildListItem(
  patient: DemoPatientRecord,
  nowMs: number,
  language: DemoLanguage,
): PatientListItem {
  const t = dataTimeMs(patient, nowMs);
  const readingsExist = hasReadings(patient);
  const vitals = readingsExist ? vitalsAt(patient, t) : { heartRate: 0, spo2: 0 };
  const risk = readingsExist ? riskAt(patient, t) : null;
  const statusLevel = risk?.statusLevel ?? 'Stable';
  // A stable patient has no alert; every other status is clinical unless it is an electrode/device problem.
  const alertKind: PatientListItem['alertKind'] =
    statusLevel === 'Stable' ? undefined : (patient.alertKind ?? 'Clinical');

  return {
    id: patient.id,
    fullName: patient.fullName,
    age: patient.age,
    diagnosis: patient.diagnosis,
    fileNumber: patient.fileNumber,
    bedOrRoom: patient.bedOrRoom,
    statusLevel,
    statusLabel: statusLabelText(statusLabelKey(patient), language),
    alertKind,
    isLive: isDeviceLive(patient),
    heartRate: vitals.heartRate,
    heartRateRange: heartRateRange24h(patient, nowMs),
    spo2: vitals.spo2,
    riskScore: risk?.riskScore,
    lastUpdatedAt: iso(t),
  };
}

/** Ward statistic tiles; every number is computed from THIS doctor's patients only. */
export function buildWardStats(db: DemoDatabase, doctorId: string, nowMs: number): WardStats {
  const patients = patientsOfDoctor(db, doctorId);
  let stableCount = 0;
  let cautionCount = 0;
  let criticalCount = 0;
  let totalDevices = 0;
  let connectedDevices = 0;
  let confidenceSum = 0;
  let confidenceSamples = 0;

  for (const patient of patients) {
    const level = statusLevelOf(patient) ?? 'Stable';
    if (level === 'Critical') criticalCount += 1;
    else if (level === 'Caution') cautionCount += 1;
    else stableCount += 1;

    if (deviceOf(db, patient.id)) totalDevices += 1;
    if (isDeviceLive(patient)) connectedDevices += 1;
    if (hasReadings(patient)) {
      confidenceSum += electrodeAt(patient, dataTimeMs(patient, nowMs)).confidenceScore;
      confidenceSamples += 1;
    }
  }

  return {
    activePatients: patients.length,
    wardCapacity: DEMO_WARD_CAPACITY,
    criticalCasesCount: criticalCount,
    criticalCapacityTotal: DEMO_CRITICAL_CAPACITY,
    connectedDevices,
    totalDevices,
    deviceLinkRatePercent:
      totalDevices === 0 ? 0 : Math.round((connectedDevices / totalDevices) * 100),
    signalAccuracyPercent:
      confidenceSamples === 0 ? 0 : roundTo((confidenceSum / confidenceSamples) * 100, 1),
    stableCount,
    cautionCount,
  };
}

// ---------------------------------------------------------------------------
// History and AI report
// ---------------------------------------------------------------------------

/** `GET /patients/{id|me}/vitals-history`: one real-looking sample per time bucket. */
export function buildVitalsHistory(
  db: DemoDatabase,
  patient: DemoPatientRecord,
  range: HistoryRangeKey,
  nowMs: number,
): VitalsSample[] {
  const deviceId = deviceOf(db, patient.id)?.serialNumber ?? 'demo-device';
  return historySeries(patient, range, nowMs).map<VitalsSample>((point) => ({
    id: `${patient.id}-history-${point.timestampMs}`,
    patientId: patient.id,
    deviceId,
    timestamp: iso(point.timestampMs),
    heartRate: point.heartRate,
    spo2: point.spo2,
  }));
}

const PERIOD_TO_RANGE: Record<AiReportPeriod, HistoryRangeKey> = {
  daily: '24h',
  weekly: '7d',
  monthly: '30d',
};

/** How far above the baseline score the "highest risk reading" of a period peaks, per scenario. */
const RISK_PEAK_BONUS = { stable: 8, caution: 12, critical: 6 } as const;

/** `GET /patients/{id|me}/ai-report`; null when the patient has no readings yet (the real API answers 404). */
export function buildAiReport(
  db: DemoDatabase,
  patient: DemoPatientRecord,
  period: AiReportPeriod,
  viewer: UserRole,
  nowMs: number,
  language: DemoLanguage,
): AiReport | null {
  if (patient.scenario === 'idle') return null;

  const range = PERIOD_TO_RANGE[period];
  const series = historySeries(patient, range, nowMs);
  if (series.length === 0) return null;

  const totalHeartRate = series.reduce((sum, point) => sum + point.heartRate, 0);
  const averageHeartRate = Math.round(totalHeartRate / series.length);
  const windowStart = nowMs - historySpanMs(range);
  const alertsLoggedCount = buildAlerts(db, patient, nowMs, language).filter(
    (alert) => Date.parse(alert.triggeredAt) >= windowStart,
  ).length;
  const highestRiskReading = Math.min(
    99,
    patient.baseline.riskScore + RISK_PEAK_BONUS[patient.scenario],
  );

  return {
    period,
    narrativeSummary: buildNarrative({
      scenario: patient.scenario,
      technicalAlert: patient.alertKind === 'Technical',
      period,
      averageHeartRate,
      alertCount: alertsLoggedCount,
      viewer,
      language,
    }),
    averageHeartRate,
    highestRiskReading,
    alertsLoggedCount,
    generatedAt: iso(nowMs),
  };
}
