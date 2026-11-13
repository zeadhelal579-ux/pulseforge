/**
 * Shared API types.
 *
 * Every type here is meant to mirror something concrete on the backend side:
 *  - "Entities" mirror backend-database-architecture.md §6 table definitions.
 *  - "DTOs" mirror the request/response bodies implied by §5's endpoint table.
 *  - "Realtime payloads" mirror the SignalR events listed in §5.
 *
 * Where the backend doc doesn't fully specify a shape, that's called out in a
 * comment starting with "ASSUMPTION:" — search for that word before wiring up
 * the real .NET endpoints, since those are the spots that need a decision
 * (or a quick confirmation) from whoever builds the API.
 */

// ---------------------------------------------------------------------------
// Enums (mirrors Domain/Enums/*.cs)
// ---------------------------------------------------------------------------

export type UserRole = 'Patient' | 'Doctor';

export type ConnectionMode = 'WiFi' | 'Bluetooth';

/** The only thing a patient's UI is ever allowed to render — never a raw score. */
export type RiskStatusLevel = 'Stable' | 'Caution' | 'Critical';

/**
 * ASSUMPTION: the backend doc defines an `AlertSeverity` enum (Domain/Enums)
 * but doesn't enumerate its members. The four values below cover every alert
 * badge seen in the reviewed mockups ("Moderate Alert", "Transient",
 * "Optimal", and implicitly "Critical"). Confirm/adjust to match the real
 * enum once it exists in CardioSense.Domain.
 */
export type AlertSeverity = 'Info' | 'Transient' | 'Moderate' | 'Critical';

export type MedicalHistoryEntryType = 'Diagnosis' | 'Medication' | 'Condition';

export type Gender = 'male' | 'female';

// ---------------------------------------------------------------------------
// Core entities
// ---------------------------------------------------------------------------

/**
 * Full aggregate returned by `GET /api/patients/me` and `GET /api/patients/{id}`.
 *
 * ASSUMPTION: backend-database-architecture.md §5's Application layer lists a
 * single `GetPatientProfile` query (no separate GetLatestVitals/GetElectrodeStatus
 * queries), and explicitly says the doctor's by-id endpoint returns "the same
 * shape" as `me` — so this DTO is modeled as one aggregate covering everything
 * the detail page renders *right now* (profile + latest reading + latest AI
 * assessment + electrode/device status + medical history + the 24h timeline).
 * Historical series (`vitals-history`, `alerts`) stay separate, paginated
 * endpoints since those grow unbounded over time.
 */
export interface PatientProfile {
  id: string;
  fullName: string;
  age: number;
  gender: Gender;
  nationalId: string;
  phone: string;
  email: string;
  diagnosis: string;
  medications?: string;
  /** Present once a doctor has actually been linked via DoctorPatientAssignment. */
  assignedDoctor?: {
    id: string;
    fullName: string;
    specialty: string;
    /** Needed for the patient dashboard's "Call my doctor" tel: link. */
    phone: string;
  } | null;
  device?: DeviceSummary | null;
  preferredLanguage: 'ar' | 'en';
  medicalHistory: MedicalHistoryEntry[];

  /** Null until the device has sent at least one reading — the empty state
   *  the dashboard renders instead of ever inventing sample numbers. */
  latestVitals: VitalsSample | null;
  latestEcgFeatureSet: EcgFeatureSet | null;
  latestRiskAssessment: RiskAssessment | null;
  latestElectrodeVerification: ElectrodeVerification | null;
  hourlyStatusTimeline: HourlyStatusBucket[];
}

export interface DoctorProfile {
  id: string;
  fullName: string;
  specialty: string;
  licenseNumber: string;
  hospital: string;
  department?: string;
  phone: string;
  email: string;
  preferredLanguage: 'ar' | 'en';
}

export interface DeviceSummary {
  serialNumber: string;
  firmwareVersion: string;
  batteryLevel: number; // 0-100
  lastSeenAt: string; // ISO timestamp
  connectionMode: ConnectionMode;
  /** Round-trip signal strength shown next to the connection mode, e.g. "-62 dBm". */
  signalStrengthDbm?: number;
}

export interface MedicalHistoryEntry {
  id: string;
  entryType: MedicalHistoryEntryType;
  description: string;
  recordedAt: string;
}

export interface VitalsSample {
  id: string;
  patientId: string;
  deviceId: string;
  timestamp: string;
  heartRate: number;
  spo2: number;
}

export interface EcgFeatureSet {
  id: string;
  patientId: string;
  timestamp: string;
  prIntervalMs: number;
  qrsDurationMs: number;
  /** [QT, QTc] in milliseconds — matches the AI service contract's `qtQtcMs` field. */
  qtQtcMs: [number, number];
}

export interface RiskAssessment {
  id: string;
  patientId: string;
  timestamp: string;
  statusLevel: RiskStatusLevel;
  /** Doctor-only per the product decision in PROJECT_OVERVIEW.md §4 — the UI layer
   *  (not the API) is responsible for hiding this from patient-role views. */
  confidenceScore: number;
  modelVersion: string;
  /** Doctor-only flags, e.g. ["possible_st_elevation"]. Empty array when none apply. */
  flags: string[];
  /** ASSUMPTION: doctor-only numeric 0-100 risk score shown on the mockups
   *  (e.g. "RISK SCORE 18/100"). Not explicitly named as a column in the
   *  RiskAssessments table (§6) — likely stored as part of that row or
   *  derived from confidenceScore. Confirm the real field name with backend. */
  riskScore?: number;
}

export interface Alert {
  id: string;
  patientId: string;
  type: string;
  severity: AlertSeverity;
  message: string;
  triggeredAt: string;
  resolvedAt?: string | null;
  resolutionNote?: string | null;
}

export interface ElectrodeVerification {
  id: string;
  patientId: string;
  timestamp: string;
  /** e.g. "Optimal Placement", "Reposition Needed" — exact string set owned by backend. */
  placementStatus: string;
  confidenceScore: number;
  /** Impedance readings per lead, e.g. { RA: 1.8, LA: 2.0, LL: 1.9 } in kΩ. */
  impedanceKOhm?: Record<string, number>;
}

/** One bucket of the 24-hour hourly status timeline strip. */
export interface HourlyStatusBucket {
  hour: number; // 0-23
  status: RiskStatusLevel;
  isCurrent: boolean;
}

// ---------------------------------------------------------------------------
// Auth DTOs — POST /api/auth/*
// ---------------------------------------------------------------------------

export interface LoginRequest {
  email: string;
  password: string;
}

export interface PatientSignupRequest {
  fullName: string;
  phone: string;
  email: string;
  password: string;
  age: number;
  gender: Gender;
  nationalId: string;
  diagnosis: string;
  medications?: string;
  /** Free-text for now; frontend-architecture.md §11 flags this as a likely
   *  future dropdown once a searchable doctor directory exists. */
  assignedDoctorName?: string;
  deviceSerial?: string;
  preferredLanguage: 'ar' | 'en';
}

export interface DoctorSignupRequest {
  fullName: string;
  phone: string;
  email: string;
  password: string;
  specialty: string;
  licenseNumber: string;
  hospital: string;
  department?: string;
  preferredLanguage: 'ar' | 'en';
}

/** The authenticated user's session-relevant fields — kept intentionally small. */
export interface AuthenticatedUser {
  id: string;
  role: UserRole;
  fullName: string;
  email: string;
  preferredLanguage: 'ar' | 'en';
  /** Present only when role === 'Patient'; lets guards resolve "my own" routes. */
  patientId?: string;
  /** Present only when role === 'Doctor'. */
  doctorId?: string;
  /** Present only when role === 'Doctor' — shown in the doctor layout's header/sidebar
   *  ("Dr. Sara El-Mansour, Cardiology Consultant") without a second profile fetch. */
  specialty?: string;
}

/**
 * Response shape for login and both signup endpoints. The refresh token itself
 * is never in this body — the backend sets it as an httpOnly cookie
 * (backend-database-architecture.md §8), so JavaScript never sees it.
 */
export interface AuthResponse {
  accessToken: string;
  user: AuthenticatedUser;
}

export interface RefreshResponse {
  accessToken: string;
}

// ---------------------------------------------------------------------------
// Doctor-side DTOs
// ---------------------------------------------------------------------------

export type PatientListStatusFilter = 'all' | 'stable' | 'alert' | 'critical';

export interface PatientListItem {
  id: string;
  fullName: string;
  age: number;
  diagnosis: string;
  fileNumber: string; // e.g. "CS-8921"
  bedOrRoom?: string; // e.g. "CCU-04" or "W-204"
  statusLevel: RiskStatusLevel;
  /** Human-readable label shown on the pill, e.g. "Fully Stable", "Clinical Alert",
   *  "Technical Alert" — lets the backend distinguish a cardiac alert from a
   *  device/lead-quality one while the frontend still colors by statusLevel. */
  statusLabel: string;
  alertKind?: 'Clinical' | 'Technical';
  isLive: boolean;
  heartRate: number;
  heartRateRange: { min: number; max: number };
  spo2: number;
  bloodPressure?: string;
  riskScore?: number;
  stElevationMm?: number;
  lastUpdatedAt: string;
}

export interface WardStats {
  activePatients: number;
  wardCapacity: number;
  criticalCasesCount: number;
  criticalCapacityTotal: number;
  connectedDevices: number;
  totalDevices: number;
  deviceLinkRatePercent: number;
  signalAccuracyPercent: number;
  /** Global counts across all of this doctor's patients, independent of the
   *  current search/filter/page — feeds the "All 42 / Stable 34 / Alert 6 /
   *  Critical 2" segmented control on the patient list. */
  stableCount: number;
  cautionCount: number;
}

export interface PaginatedResponse<T> {
  items: T[];
  page: number;
  pageSize: number;
  totalCount: number;
  totalPages: number;
}

export interface AddPatientRequest {
  /** National ID or invite code — either links an existing patient account. */
  identifier: string;
}

export interface LogManualEventRequest {
  note: string;
  severity: AlertSeverity;
}

export interface FreezeWaveformRequest {
  frozen: boolean;
}

// ---------------------------------------------------------------------------
// Device DTOs
// ---------------------------------------------------------------------------

export interface PairDeviceRequest {
  serialNumber: string;
  patientId?: string;
}

export interface DeviceStatusResponse extends DeviceSummary {
  serialNumber: string;
}

// ---------------------------------------------------------------------------
// Realtime (SignalR) payloads — backend-database-architecture.md §5 "SignalR hub"
// ---------------------------------------------------------------------------

export interface VitalsTickPayload {
  patientId: string;
  heartRate: number;
  spo2: number;
  timestamp: string;
}

export interface RiskStatusPayload {
  patientId: string;
  statusLevel: RiskStatusLevel;
  confidenceScore: number;
  riskScore?: number;
  flags: string[];
  timestamp: string;
}

export interface PatientSummaryUpdate {
  patientId: string;
  statusLevel: RiskStatusLevel;
  statusLabel: string;
  heartRate: number;
  spo2: number;
  isLive: boolean;
}

// ---------------------------------------------------------------------------
// AI narrative report
//
// ASSUMPTION / GAP: none of backend-database-architecture.md §5's endpoints
// cover the "CardioSense AI Clinical Reports" narrative card visible in the
// reviewed mockups (daily/weekly/monthly summary + average HR + highest risk
// reading + alert count). The frontend below calls a proposed
// `GET /api/patients/{id}/ai-report?period=` (and its `/me` twin) — this
// endpoint needs to be added on the backend; it doesn't exist yet.
// ---------------------------------------------------------------------------

export type AiReportPeriod = 'daily' | 'weekly' | 'monthly';

export interface AiReport {
  period: AiReportPeriod;
  /** Short natural-language summary, e.g. "Over the past 24 hours...". Generated
   *  by the AI service, displayed verbatim — the frontend never fabricates this text. */
  narrativeSummary: string;
  averageHeartRate: number;
  highestRiskReading: number | null;
  alertsLoggedCount: number;
  generatedAt: string;
}

// ---------------------------------------------------------------------------
// Error shape — backend-database-architecture.md §13 (RFC 7807 ProblemDetails)
// ---------------------------------------------------------------------------

export interface ProblemDetails {
  title: string;
  status: number;
  detail?: string;
  /** Field-level validation errors from FluentValidation's pipeline behavior. */
  errors?: Record<string, string[]>;
}
