import type { Alert, PatientListItem, PatientProfile } from '@/shared/types/api';

/**
 * Mirrors the shape of "Ahmed Mostafa Kamal" from the reviewed CardioSense
 * mockups — used only in tests, to check components render correctly
 * against realistic data, never imported by app code.
 */
export const sampleProfileFixture: PatientProfile = {
  id: 'patient-1',
  fullName: 'Ahmed Mostafa Kamal',
  age: 58,
  gender: 'male',
  nationalId: '29001011234567',
  phone: '01012345678',
  email: 'ahmed@example.com',
  diagnosis: 'Prior Myocardial Infarction / Arrhythmia Follow-up',
  medications: 'Bisoprolol 5mg, Aspirin 100mg',
  assignedDoctor: {
    id: 'doctor-1',
    fullName: 'Dr. Sara El-Mansour',
    specialty: 'Cardiology Consultant',
    phone: '01098765432',
  },
  device: {
    serialNumber: 'CS-94281-EG',
    firmwareVersion: '1.2.0',
    batteryLevel: 84,
    lastSeenAt: '2026-09-13T08:00:00Z',
    connectionMode: 'Bluetooth',
    signalStrengthDbm: -62,
  },
  preferredLanguage: 'en',
  medicalHistory: [
    { id: 'mh-1', entryType: 'Diagnosis', description: 'Hypertension', recordedAt: '2021-01-01' },
    { id: 'mh-2', entryType: 'Medication', description: 'Bisoprolol 5mg', recordedAt: '2021-01-01' },
  ],
  latestVitals: {
    id: 'vs-1',
    patientId: 'patient-1',
    deviceId: 'device-1',
    timestamp: '2026-09-13T08:00:00Z',
    heartRate: 74,
    spo2: 98,
  },
  latestEcgFeatureSet: {
    id: 'ecg-1',
    patientId: 'patient-1',
    timestamp: '2026-09-13T08:00:00Z',
    prIntervalMs: 152,
    qrsDurationMs: 88,
    qtQtcMs: [396, 418],
  },
  latestRiskAssessment: {
    id: 'risk-1',
    patientId: 'patient-1',
    timestamp: '2026-09-13T08:00:00Z',
    statusLevel: 'Stable',
    confidenceScore: 0.94,
    modelVersion: 'v0.3-prototype',
    flags: [],
    riskScore: 18,
  },
  latestElectrodeVerification: {
    id: 'ev-1',
    patientId: 'patient-1',
    timestamp: '2026-09-13T08:00:00Z',
    placementStatus: 'Verified via Camera — Optimal Placement',
    confidenceScore: 0.98,
    impedanceKOhm: { RA: 1.8, LA: 2.0, LL: 1.9 },
  },
  hourlyStatusTimeline: Array.from({ length: 24 }, (_, hour) => ({
    hour,
    status: hour === 5 || hour === 7 ? 'Caution' : 'Stable',
    isCurrent: hour === 8,
  })),
};

export const sampleAlertFixture: Alert = {
  id: 'alert-1',
  patientId: 'patient-1',
  type: 'Transient Heart Rate Increase',
  severity: 'Moderate',
  message: 'Transient Heart Rate Increase (96 BPM)',
  triggeredAt: '2026-09-13T07:14:00Z',
  resolvedAt: null,
  resolutionNote: null,
};

export const samplePatientListItemFixture: PatientListItem = {
  id: 'patient-1',
  fullName: 'Ahmed Mostafa Kamal',
  age: 58,
  diagnosis: 'Prior Myocardial Infarction (Post-MI)',
  fileNumber: 'CS-8921',
  bedOrRoom: 'CCU-04',
  statusLevel: 'Stable',
  statusLabel: 'Fully Stable',
  isLive: false,
  heartRate: 74,
  heartRateRange: { min: 60, max: 100 },
  spo2: 98,
  lastUpdatedAt: '2026-09-13T08:00:00Z',
};
