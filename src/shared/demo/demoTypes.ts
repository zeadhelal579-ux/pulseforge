/**
 * File: src/shared/demo/demoTypes.ts
 * Layer: shared/demo (in-browser demo backend)
 * Purpose: Internal record shapes of the demo database plus the tiny request/response envelope that the
 *          router exposes to the Axios adapter.
 * Logic: Records are JSON-serialisable (they live in localStorage). Everything the dashboards render but that
 *        changes with time (vitals, risk, timeline, history) is NOT stored; it is derived from the patient's
 *        baseline by demoReadings.ts, which keeps the stored database tiny and always "fresh".
 * Implementation: interfaces only; public DTOs are reused from shared/types/api.ts so the demo cannot drift
 *        from the contract the real backend must follow.
 */
import type {
  Alert,
  ConnectionMode,
  Gender,
  MedicalHistoryEntry,
  UserRole,
} from '@/shared/types/api';

/** How a simulated patient behaves; `idle` = no device paired yet, so no readings exist at all. */
export type DemoScenario = 'stable' | 'caution' | 'critical' | 'idle';

/** Resting values the live simulation oscillates around. */
export interface DemoBaseline {
  heartRate: number;
  spo2: number;
  /** Doctor-only 0-100 score. */
  riskScore: number;
  /** 0-1 model confidence (doctor-only). */
  confidence: number;
}

export interface DemoPatientRecord {
  id: string;
  /** Null for filler patients that have no login. */
  userId: string | null;
  fileNumber: string;
  fullName: string;
  age: number;
  gender: Gender;
  nationalId: string;
  phone: string;
  email: string;
  diagnosis: string;
  medications?: string;
  medicalHistory: MedicalHistoryEntry[];
  scenario: DemoScenario;
  /** Only meaningful for `caution`: a cardiac alert vs a device/electrode-quality alert. */
  alertKind: 'Clinical' | 'Technical' | null;
  baseline: DemoBaseline;
  bedOrRoom?: string;
  /** > 0 simulates a device that stopped transmitting this many minutes ago. */
  offlineMinutes: number;
  /**
   * Epoch ms of the first reading for a device paired in the browser; null means "a full simulated history
   * exists" (seeded patients). Keeps a freshly paired device from showing 30 days of data it never recorded.
   */
  dataSinceMs: number | null;
  createdAt: string;
}

export interface DemoDoctorRecord {
  id: string;
  userId: string;
  fullName: string;
  specialty: string;
  licenseNumber: string;
  hospital: string;
  department?: string;
  phone: string;
  email: string;
}

/** `demo` = seeded account whose password is the public demo password; `hashed` = account created in the browser. */
export type DemoCredential = { kind: 'demo' } | { kind: 'hashed'; salt: string; hash: string };

export interface DemoUserRecord {
  id: string;
  email: string;
  role: UserRole;
  /** Id of the matching patient or doctor record. */
  profileId: string;
  preferredLanguage: 'ar' | 'en';
  credential: DemoCredential;
}

export interface DemoAssignment {
  doctorId: string;
  patientId: string;
  assignedAt: string;
  isPrimary: boolean;
}

export interface DemoDeviceRecord {
  serialNumber: string;
  patientId: string | null;
  pairedAt: string | null;
  firmwareVersion: string;
  batteryLevel: number;
  connectionMode: ConnectionMode;
  signalStrengthDbm: number;
}

/** The complete persisted database. */
export interface DemoDatabase {
  version: number;
  seededAt: string;
  users: DemoUserRecord[];
  patients: DemoPatientRecord[];
  doctors: DemoDoctorRecord[];
  assignments: DemoAssignment[];
  devices: DemoDeviceRecord[];
  /** Alerts created at runtime (manual events); seeded alerts are generated on read. */
  manualAlerts: Alert[];
}

/** Emulated refresh cookie. */
export interface DemoSession {
  userId: string;
  expiresAt: number;
}

export type DemoMethod = 'GET' | 'POST' | 'PUT' | 'PATCH' | 'DELETE';

/** What the Axios adapter hands to the router. */
export interface DemoRequest {
  method: DemoMethod;
  /** Path relative to the API base, e.g. `/patients/me/alerts`. */
  path: string;
  query: Record<string, string>;
  body: unknown;
  /** Raw `Authorization` header value or null. */
  authorization: string | null;
  nowMs: number;
}

/** What the router returns; the adapter turns it into an Axios response or error. */
export interface DemoResponse {
  status: number;
  data: unknown;
}
