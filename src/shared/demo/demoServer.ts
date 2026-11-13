/**
 * File: src/shared/demo/demoServer.ts
 * Layer: shared/demo (in-browser demo backend)
 * Purpose: A tiny in-browser REST "server" implementing the same endpoints, status codes, error shapes and
 *          access rules the frontend expects from the real ASP.NET Core API (see backend-database-architecture.md
 *          section 5 and BACKEND_REPORT.md section 5.3).
 * Logic:
 *  - Framework-free: takes a plain DemoRequest and returns a plain DemoResponse, so it is trivial to unit-test
 *    and the Axios adapter stays a thin wrapper.
 *  - Enforces the product's authorisation rules: a patient reaches only their own record; a doctor reaches only
 *    patients assigned to them; clinical actions (manual events, waveform freeze) are doctor-only; failures use
 *    RFC 7807-style ProblemDetails bodies.
 *  - Every handler reads the database, mutates it synchronously and saves it, so handlers never interleave.
 * Implementation: a route table of regex matchers, one small function per endpoint, shared input parsers.
 */
import { DEMO_PAGE_SIZE, REFRESH_SESSION_TTL_MS } from '@/shared/demo/demoConstants';
import {
  bearerToken,
  createAccessToken,
  createHashedCredential,
  newId,
  parseAccessToken,
  verifyCredential,
  type DemoTokenClaims,
} from '@/shared/demo/demoAuth';
import { hash01, roundTo, seedFromString } from '@/shared/demo/demoMath';
import {
  buildAiReport,
  buildAlerts,
  buildAuthenticatedUser,
  buildDeviceSummary,
  buildDoctorProfile,
  buildListItem,
  buildPatientProfile,
  buildVitalsHistory,
  buildWardStats,
  deviceOf,
  isAssigned,
  patientsOfDoctor,
} from '@/shared/demo/demoModel';
import { isHistoryRange, statusLevelOf } from '@/shared/demo/demoReadings';
import {
  clearDemoSession,
  loadDemoDb,
  readDemoSession,
  saveDemoDb,
  writeDemoSession,
} from '@/shared/demo/demoStorage';
import { ALERT_TYPE_MANUAL, alertTypeText, currentLanguage } from '@/shared/demo/demoText';
import type {
  DemoBaseline,
  DemoDatabase,
  DemoDeviceRecord,
  DemoDoctorRecord,
  DemoMethod,
  DemoPatientRecord,
  DemoRequest,
  DemoResponse,
  DemoUserRecord,
} from '@/shared/demo/demoTypes';
import type {
  AiReportPeriod,
  Alert,
  AlertSeverity,
  AuthResponse,
  Gender,
  MedicalHistoryEntry,
  PaginatedResponse,
  PatientListItem,
  PatientListStatusFilter,
  ProblemDetails,
  RiskStatusLevel,
} from '@/shared/types/api';

// ---------------------------------------------------------------------------
// Context and routing types
// ---------------------------------------------------------------------------

interface Authed {
  user: DemoUserRecord;
  claims: DemoTokenClaims;
}

interface BaseContext {
  request: DemoRequest;
  params: Record<string, string>;
  db: DemoDatabase;
}

interface ProtectedContext extends BaseContext {
  auth: Authed;
}

type MaybeAsync<T> = T | Promise<T>;

interface RouteBase {
  method: DemoMethod;
  matcher: RegExp;
  names: string[];
}

type Route =
  | (RouteBase & { kind: 'public'; handler: (ctx: BaseContext) => MaybeAsync<DemoResponse> })
  | (RouteBase & {
      kind: 'protected';
      handler: (ctx: ProtectedContext) => MaybeAsync<DemoResponse>;
    });

type FieldErrors = Record<string, string[]>;

// ---------------------------------------------------------------------------
// Response helpers
// ---------------------------------------------------------------------------

/** Successful response. */
function json(status: number, data: unknown): DemoResponse {
  return { status, data };
}

/** RFC 7807-style error response, the same shape the real API's ProblemDetails middleware produces. */
function problem(
  status: number,
  title: string,
  detail?: string,
  errors?: FieldErrors,
): DemoResponse {
  const body: ProblemDetails = { title, status };
  if (detail) body.detail = detail;
  if (errors) body.errors = errors;
  return { status, data: body };
}

const FORBIDDEN_PATIENT = 'You do not have access to this patient.';
const DOCTOR_ONLY = 'This action is reserved for doctors.';

// ---------------------------------------------------------------------------
// Input parsing helpers (request bodies arrive as `unknown`)
// ---------------------------------------------------------------------------

/** Narrows an unknown body to a plain object (empty object otherwise). */
function asRecord(value: unknown): Record<string, unknown> {
  return typeof value === 'object' && value !== null && !Array.isArray(value)
    ? (value as Record<string, unknown>)
    : {};
}

/** Trimmed string, or '' when the value is not a string. */
function asText(value: unknown): string {
  return typeof value === 'string' ? value.trim() : '';
}

/** Finite number from a number or numeric string, else null. */
function asNumber(value: unknown): number | null {
  if (typeof value === 'number' && Number.isFinite(value)) return value;
  if (typeof value === 'string' && value.trim() !== '') {
    const parsed = Number(value);
    return Number.isFinite(parsed) ? parsed : null;
  }
  return null;
}

/** Lower-cased, trimmed email used as the account key. */
function normalizeEmail(value: unknown): string {
  return asText(value).toLowerCase();
}

/** Doctor-name comparison key: case, spacing and a leading "Dr." are ignored. */
function normalizeName(value: string): string {
  return value
    .toLowerCase()
    .replace(/\s+/g, ' ')
    .replace(/^(dr\.?|د\.?)\s*/, '')
    .trim();
}

/** Appends a field-level validation message. */
function addError(errors: FieldErrors, field: string, message: string): void {
  errors[field] = [...(errors[field] ?? []), message];
}

const EMAIL_PATTERN = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;

/** Reads a language from a body value, defaulting to the language currently shown in the UI. */
function languageFrom(value: unknown): 'ar' | 'en' {
  if (value === 'ar' || value === 'en') return value;
  return currentLanguage();
}

/** Route parameter lookup that never yields undefined. */
function param(ctx: BaseContext, name: string): string {
  return ctx.params[name] ?? '';
}

// ---------------------------------------------------------------------------
// Authentication and authorisation
// ---------------------------------------------------------------------------

type AuthResult = { ok: true; auth: Authed } | { ok: false; response: DemoResponse };

/** Validates the bearer token and resolves the calling user. */
function authenticate(request: DemoRequest, db: DemoDatabase): AuthResult {
  const token = bearerToken(request.authorization);
  if (!token) return { ok: false, response: problem(401, 'Unauthorized', 'Missing access token.') };

  const claims = parseAccessToken(token, request.nowMs);
  if (claims === 'expired') {
    return { ok: false, response: problem(401, 'Unauthorized', 'Access token expired.') };
  }
  if (!claims) {
    return { ok: false, response: problem(401, 'Unauthorized', 'Invalid access token.') };
  }

  const user = db.users.find((candidate) => candidate.id === claims.sub);
  if (!user) return { ok: false, response: problem(401, 'Unauthorized', 'Unknown user.') };
  return { ok: true, auth: { user, claims } };
}

/** The single rule deciding "can this caller see this patient?": self for patients, assigned for doctors. */
function canAccessPatient(db: DemoDatabase, auth: Authed, patientId: string): boolean {
  if (auth.claims.role === 'Patient') return auth.claims.patientId === patientId;
  return auth.claims.doctorId !== undefined && isAssigned(db, auth.claims.doctorId, patientId);
}

type PatientLookup = { patient: DemoPatientRecord } | { error: DemoResponse };

/** Resolves `me` or an explicit id to a patient the caller may access. */
function resolvePatient(ctx: ProtectedContext, target: string): PatientLookup {
  let patientId = target;
  if (target === 'me') {
    if (ctx.auth.claims.role !== 'Patient' || !ctx.auth.claims.patientId) {
      return { error: problem(403, 'Forbidden', 'Only patients have a personal profile.') };
    }
    patientId = ctx.auth.claims.patientId;
  }
  if (!canAccessPatient(ctx.db, ctx.auth, patientId)) {
    return { error: problem(403, 'Forbidden', FORBIDDEN_PATIENT) };
  }
  const patient = ctx.db.patients.find((candidate) => candidate.id === patientId);
  return patient ? { patient } : { error: problem(404, 'Not Found', 'Patient not found.') };
}

/** Builds the access token for a user (claims mirror the real backend's JWT). */
function createTokenFor(user: DemoUserRecord, nowMs: number): string {
  return createAccessToken(
    {
      sub: user.id,
      role: user.role,
      ...(user.role === 'Patient' ? { patientId: user.profileId } : { doctorId: user.profileId }),
    },
    nowMs,
  );
}

/** Opens the emulated refresh session and returns the login/signup response body. */
function issueSession(db: DemoDatabase, user: DemoUserRecord, nowMs: number): AuthResponse {
  writeDemoSession({ userId: user.id, expiresAt: nowMs + REFRESH_SESSION_TTL_MS });
  return { accessToken: createTokenFor(user, nowMs), user: buildAuthenticatedUser(db, user) };
}

// ---------------------------------------------------------------------------
// Auth endpoints
// ---------------------------------------------------------------------------

/** POST /auth/login */
async function login(ctx: BaseContext): Promise<DemoResponse> {
  const body = asRecord(ctx.request.body);
  const email = normalizeEmail(body.email);
  const password = typeof body.password === 'string' ? body.password : '';
  if (!email || !password) {
    return problem(400, 'Validation failed', undefined, {
      email: ['Required.'],
      password: ['Required.'],
    });
  }

  const user = ctx.db.users.find((candidate) => candidate.email === email);
  // Same error for "unknown email" and "wrong password": the response must not reveal who is registered.
  if (!user || !(await verifyCredential(user.credential, password))) {
    return problem(401, 'Invalid credentials', 'Incorrect email or password.');
  }

  // The demo keeps whatever language the visitor is browsing in, instead of snapping back to a stored value.
  const language = currentLanguage();
  if (user.preferredLanguage !== language) {
    user.preferredLanguage = language;
    saveDemoDb(ctx.db);
  }
  return json(200, issueSession(ctx.db, user, ctx.request.nowMs));
}

/** Resting values for an account created in the browser (deterministic per patient id). */
function createBaseline(seed: number): DemoBaseline {
  return {
    heartRate: 66 + Math.floor(hash01(seed, 1) * 16),
    spo2: 96 + Math.floor(hash01(seed, 2) * 3),
    riskScore: 10 + Math.floor(hash01(seed, 3) * 14),
    confidence: roundTo(0.9 + hash01(seed, 4) * 0.06, 2),
  };
}

/** Next clinical file number (CS-8914, CS-8915, ...). */
function nextFileNumber(db: DemoDatabase): string {
  let highest = 8900;
  for (const patient of db.patients) {
    const match = /^CS-(\d+)$/.exec(patient.fileNumber);
    if (match?.[1]) highest = Math.max(highest, Number(match[1]));
  }
  return `CS-${highest + 1}`;
}

type PairOutcome =
  | { ok: true; device: DemoDeviceRecord }
  | { ok: false; reason: 'invalidSerial' | 'deviceTaken' | 'patientHasDevice' };

/**
 * Pairs a device with a patient (domain rules from Device.PairTo: one device per patient, one patient per
 * device). Demo simplification: an unknown but well-formed serial is accepted and added to the inventory.
 * The first pairing flips an "idle" patient to a live simulated patient.
 */
function pairDeviceToPatient(
  db: DemoDatabase,
  patient: DemoPatientRecord,
  rawSerial: string,
  nowMs: number,
): PairOutcome {
  const serial = rawSerial.trim().toUpperCase();
  if (!/^[A-Z0-9-]{4,40}$/.test(serial)) return { ok: false, reason: 'invalidSerial' };

  const ownDevice = deviceOf(db, patient.id);
  if (ownDevice && ownDevice.serialNumber !== serial) {
    return { ok: false, reason: 'patientHasDevice' };
  }

  let device = db.devices.find((candidate) => candidate.serialNumber === serial);
  if (device?.patientId && device.patientId !== patient.id) {
    return { ok: false, reason: 'deviceTaken' };
  }

  if (!device) {
    device = {
      serialNumber: serial,
      patientId: null,
      pairedAt: null,
      firmwareVersion: '1.4.2',
      batteryLevel: 100,
      connectionMode: 'WiFi',
      signalStrengthDbm: -55,
    };
    db.devices.push(device);
  }
  device.patientId = patient.id;
  device.pairedAt ??= new Date(nowMs).toISOString();

  if (patient.scenario === 'idle') {
    patient.scenario = 'stable';
    patient.dataSinceMs = nowMs;
  }
  return { ok: true, device };
}

/** POST /auth/signup/patient */
async function signupPatient(ctx: BaseContext): Promise<DemoResponse> {
  const body = asRecord(ctx.request.body);
  const errors: FieldErrors = {};

  const fullName = asText(body.fullName);
  const phone = asText(body.phone);
  const email = normalizeEmail(body.email);
  const password = typeof body.password === 'string' ? body.password : '';
  const age = asNumber(body.age);
  const rawGender = body.gender;
  const gender: Gender | null = rawGender === 'male' || rawGender === 'female' ? rawGender : null;
  const nationalId = asText(body.nationalId);
  const diagnosis = asText(body.diagnosis);
  const medications = asText(body.medications);

  if (!fullName) addError(errors, 'fullName', 'Required.');
  if (!phone) addError(errors, 'phone', 'Required.');
  if (!EMAIL_PATTERN.test(email)) addError(errors, 'email', 'Enter a valid email address.');
  if (password.length < 8) addError(errors, 'password', 'Password must be at least 8 characters.');
  if (age === null || !Number.isInteger(age) || age < 0 || age > 120) {
    addError(errors, 'age', 'Invalid age.');
  }
  if (!gender) addError(errors, 'gender', 'Required.');
  if (!/^[0-9]{14}$/.test(nationalId)) {
    addError(errors, 'nationalId', 'National ID must be 14 digits.');
  }
  if (!diagnosis) addError(errors, 'diagnosis', 'Required.');
  if (Object.keys(errors).length > 0 || age === null || !gender) {
    return problem(400, 'Validation failed', undefined, errors);
  }

  if (ctx.db.users.some((user) => user.email === email)) {
    return problem(409, 'Conflict', 'An account with this email already exists.');
  }
  if (ctx.db.patients.some((patient) => patient.nationalId === nationalId)) {
    return problem(409, 'Conflict', 'A patient with this national ID is already registered.');
  }

  const nowMs = ctx.request.nowMs;
  const nowIso = new Date(nowMs).toISOString();
  const patientId = newId();
  const userId = newId();

  const history: MedicalHistoryEntry[] = [
    { id: newId(), entryType: 'Diagnosis', description: diagnosis, recordedAt: nowIso },
  ];
  if (medications) {
    history.push({
      id: newId(),
      entryType: 'Medication',
      description: medications,
      recordedAt: nowIso,
    });
  }

  const patient: DemoPatientRecord = {
    id: patientId,
    userId,
    fileNumber: nextFileNumber(ctx.db),
    fullName,
    age,
    gender,
    nationalId,
    phone,
    email,
    diagnosis,
    medications: medications || undefined,
    medicalHistory: history,
    scenario: 'idle',
    alertKind: null,
    baseline: createBaseline(seedFromString(patientId)),
    offlineMinutes: 0,
    dataSinceMs: null,
    createdAt: nowIso,
  };
  const user: DemoUserRecord = {
    id: userId,
    email,
    role: 'Patient',
    profileId: patientId,
    preferredLanguage: languageFrom(body.preferredLanguage),
    credential: await createHashedCredential(password),
  };
  ctx.db.patients.push(patient);
  ctx.db.users.push(user);

  // Optional, best-effort links (a failure here never fails the signup, like the real backend):
  // the doctor is linked only when exactly one doctor carries that exact name.
  const wantedDoctor = normalizeName(asText(body.assignedDoctorName));
  if (wantedDoctor) {
    const matches = ctx.db.doctors.filter(
      (doctor) => normalizeName(doctor.fullName) === wantedDoctor,
    );
    const onlyMatch = matches.length === 1 ? matches[0] : undefined;
    if (onlyMatch) {
      ctx.db.assignments.push({
        doctorId: onlyMatch.id,
        patientId,
        assignedAt: nowIso,
        isPrimary: true,
      });
    }
  }
  const serial = asText(body.deviceSerial);
  if (serial) pairDeviceToPatient(ctx.db, patient, serial, nowMs);

  saveDemoDb(ctx.db);
  return json(201, issueSession(ctx.db, user, nowMs));
}

/** POST /auth/signup/doctor */
async function signupDoctor(ctx: BaseContext): Promise<DemoResponse> {
  const body = asRecord(ctx.request.body);
  const errors: FieldErrors = {};

  const fullName = asText(body.fullName);
  const phone = asText(body.phone);
  const email = normalizeEmail(body.email);
  const password = typeof body.password === 'string' ? body.password : '';
  const specialty = asText(body.specialty);
  const licenseNumber = asText(body.licenseNumber);
  const hospital = asText(body.hospital);
  const department = asText(body.department);

  if (!fullName) addError(errors, 'fullName', 'Required.');
  if (!phone) addError(errors, 'phone', 'Required.');
  if (!EMAIL_PATTERN.test(email)) addError(errors, 'email', 'Enter a valid email address.');
  if (password.length < 8) addError(errors, 'password', 'Password must be at least 8 characters.');
  if (!specialty) addError(errors, 'specialty', 'Required.');
  if (!licenseNumber) addError(errors, 'licenseNumber', 'Required.');
  if (!hospital) addError(errors, 'hospital', 'Required.');
  if (Object.keys(errors).length > 0) return problem(400, 'Validation failed', undefined, errors);

  if (ctx.db.users.some((user) => user.email === email)) {
    return problem(409, 'Conflict', 'An account with this email already exists.');
  }
  const wantedLicense = licenseNumber.toLowerCase();
  if (ctx.db.doctors.some((doctor) => doctor.licenseNumber.toLowerCase() === wantedLicense)) {
    return problem(409, 'Conflict', 'A doctor with this license number is already registered.');
  }

  const doctorId = newId();
  const userId = newId();
  ctx.db.doctors.push({
    id: doctorId,
    userId,
    fullName,
    specialty,
    licenseNumber,
    hospital,
    department: department || undefined,
    phone,
    email,
  });
  const user: DemoUserRecord = {
    id: userId,
    email,
    role: 'Doctor',
    profileId: doctorId,
    preferredLanguage: languageFrom(body.preferredLanguage),
    credential: await createHashedCredential(password),
  };
  ctx.db.users.push(user);

  saveDemoDb(ctx.db);
  return json(201, issueSession(ctx.db, user, ctx.request.nowMs));
}

/** POST /auth/refresh: the emulated refresh cookie is rotated and a fresh access token returned. */
function refresh(ctx: BaseContext): DemoResponse {
  const session = readDemoSession(ctx.request.nowMs);
  if (!session) return problem(401, 'Unauthorized', 'No active demo session.');

  const user = ctx.db.users.find((candidate) => candidate.id === session.userId);
  if (!user) {
    clearDemoSession();
    return problem(401, 'Unauthorized', 'The demo account no longer exists.');
  }
  writeDemoSession({ userId: user.id, expiresAt: ctx.request.nowMs + REFRESH_SESSION_TTL_MS });
  return json(200, { accessToken: createTokenFor(user, ctx.request.nowMs) });
}

/** POST /auth/logout: ends the emulated session (idempotent). */
function logout(): DemoResponse {
  clearDemoSession();
  return json(204, undefined);
}

// ---------------------------------------------------------------------------
// Patient endpoints
// ---------------------------------------------------------------------------

/** GET /patients/{me|id} */
function getPatient(ctx: ProtectedContext): DemoResponse {
  const lookup = resolvePatient(ctx, param(ctx, 'target'));
  if ('error' in lookup) return lookup.error;
  return json(
    200,
    buildPatientProfile(ctx.db, lookup.patient, ctx.auth.claims.role, ctx.request.nowMs),
  );
}

/** PUT /patients/me: a patient may only edit their own contact details and language. */
function updateMyPatientProfile(ctx: ProtectedContext): DemoResponse {
  if (ctx.auth.claims.role !== 'Patient') {
    return problem(403, 'Forbidden', 'Only patients can edit this profile.');
  }
  const lookup = resolvePatient(ctx, 'me');
  if ('error' in lookup) return lookup.error;

  const body = asRecord(ctx.request.body);
  const fullName = asText(body.fullName);
  const phone = asText(body.phone);
  if (fullName) lookup.patient.fullName = fullName.slice(0, 120);
  if (phone) lookup.patient.phone = phone.slice(0, 20);
  if (body.preferredLanguage === 'ar' || body.preferredLanguage === 'en') {
    ctx.auth.user.preferredLanguage = body.preferredLanguage;
  }

  saveDemoDb(ctx.db);
  return json(200, buildPatientProfile(ctx.db, lookup.patient, 'Patient', ctx.request.nowMs));
}

/** GET /patients/{me|id}/vitals-history?range=24h|7d|30d */
function getVitalsHistory(ctx: ProtectedContext): DemoResponse {
  const lookup = resolvePatient(ctx, param(ctx, 'target'));
  if ('error' in lookup) return lookup.error;

  const range = ctx.request.query.range ?? '24h';
  if (!isHistoryRange(range)) {
    return problem(400, 'Validation failed', undefined, { range: ['Use 24h, 7d or 30d.'] });
  }
  return json(200, buildVitalsHistory(ctx.db, lookup.patient, range, ctx.request.nowMs));
}

/** GET /patients/{me|id}/alerts */
function getAlerts(ctx: ProtectedContext): DemoResponse {
  const lookup = resolvePatient(ctx, param(ctx, 'target'));
  if ('error' in lookup) return lookup.error;
  return json(200, buildAlerts(ctx.db, lookup.patient, ctx.request.nowMs, currentLanguage()));
}

/** GET /patients/{me|id}/ai-report?period=daily|weekly|monthly */
function getAiReport(ctx: ProtectedContext): DemoResponse {
  const lookup = resolvePatient(ctx, param(ctx, 'target'));
  if ('error' in lookup) return lookup.error;

  const period = ctx.request.query.period ?? 'daily';
  if (period !== 'daily' && period !== 'weekly' && period !== 'monthly') {
    return problem(400, 'Validation failed', undefined, {
      period: ['Use daily, weekly or monthly.'],
    });
  }
  const report = buildAiReport(
    ctx.db,
    lookup.patient,
    period as AiReportPeriod,
    ctx.auth.claims.role,
    ctx.request.nowMs,
    currentLanguage(),
  );
  // No readings yet = no report, exactly like the real endpoint: the UI renders its empty state.
  return report
    ? json(200, report)
    : problem(404, 'Not Found', 'No report data yet for this period.');
}

const ALERT_SEVERITIES: readonly AlertSeverity[] = ['Info', 'Transient', 'Moderate', 'Critical'];

/** POST /patients/{id}/manual-events (doctor only) */
function logManualEvent(ctx: ProtectedContext): DemoResponse {
  if (ctx.auth.claims.role !== 'Doctor') return problem(403, 'Forbidden', DOCTOR_ONLY);
  const lookup = resolvePatient(ctx, param(ctx, 'target'));
  if ('error' in lookup) return lookup.error;

  const body = asRecord(ctx.request.body);
  const note = asText(body.note);
  const severity = ALERT_SEVERITIES.find((candidate) => candidate === body.severity);
  const errors: FieldErrors = {};
  if (!note) addError(errors, 'note', 'Required.');
  if (!severity) addError(errors, 'severity', 'Invalid severity.');
  if (!severity || Object.keys(errors).length > 0) {
    return problem(400, 'Validation failed', undefined, errors);
  }

  const alert: Alert = {
    id: newId(),
    patientId: lookup.patient.id,
    type: ALERT_TYPE_MANUAL,
    severity,
    message: note.slice(0, 2000),
    triggeredAt: new Date(ctx.request.nowMs).toISOString(),
    resolvedAt: null,
    resolutionNote: null,
  };
  // Keep the stored list bounded so the demo database can never grow without limit.
  ctx.db.manualAlerts = [...ctx.db.manualAlerts, alert].slice(-200);
  saveDemoDb(ctx.db);
  return json(201, { ...alert, type: alertTypeText(alert.type, currentLanguage()) });
}

/** POST /patients/{id}/freeze-waveform (doctor only). Raw ECG is never stored, so this is only acknowledged. */
function freezeWaveform(ctx: ProtectedContext): DemoResponse {
  if (ctx.auth.claims.role !== 'Doctor') return problem(403, 'Forbidden', DOCTOR_ONLY);
  const lookup = resolvePatient(ctx, param(ctx, 'target'));
  if ('error' in lookup) return lookup.error;
  return json(204, undefined);
}

// ---------------------------------------------------------------------------
// Doctor endpoints
// ---------------------------------------------------------------------------

type DoctorLookup = { doctor: DemoDoctorRecord } | { error: DemoResponse };

/** Doctor profile of the caller, or an error response when the caller is not a doctor. */
function currentDoctor(ctx: ProtectedContext): DoctorLookup {
  if (ctx.auth.claims.role !== 'Doctor') return { error: problem(403, 'Forbidden', DOCTOR_ONLY) };
  const doctor = ctx.db.doctors.find((candidate) => candidate.id === ctx.auth.claims.doctorId);
  return doctor ? { doctor } : { error: problem(404, 'Not Found', 'Doctor profile not found.') };
}

/** GET /doctors/me */
function getDoctorMe(ctx: ProtectedContext): DemoResponse {
  const result = currentDoctor(ctx);
  if ('error' in result) return result.error;
  return json(200, buildDoctorProfile(result.doctor, ctx.auth.user));
}

/** PUT /doctors/me: partial update (the license number is immutable). */
function updateDoctorMe(ctx: ProtectedContext): DemoResponse {
  const result = currentDoctor(ctx);
  if ('error' in result) return result.error;

  const body = asRecord(ctx.request.body);
  const { doctor } = result;
  const fullName = asText(body.fullName);
  const specialty = asText(body.specialty);
  const hospital = asText(body.hospital);
  const phone = asText(body.phone);
  if (fullName) doctor.fullName = fullName.slice(0, 120);
  if (specialty) doctor.specialty = specialty.slice(0, 120);
  if (hospital) doctor.hospital = hospital.slice(0, 160);
  if (phone) doctor.phone = phone.slice(0, 20);
  if (body.preferredLanguage === 'ar' || body.preferredLanguage === 'en') {
    ctx.auth.user.preferredLanguage = body.preferredLanguage;
  }

  saveDemoDb(ctx.db);
  return json(200, buildDoctorProfile(doctor, ctx.auth.user));
}

const STATUS_FILTERS: readonly PatientListStatusFilter[] = ['all', 'stable', 'alert', 'critical'];

/** Maps the list filter onto the risk level it selects. */
function matchesFilter(level: RiskStatusLevel, filter: PatientListStatusFilter): boolean {
  if (filter === 'all') return true;
  if (filter === 'stable') return level === 'Stable';
  if (filter === 'alert') return level === 'Caution';
  return level === 'Critical';
}

const SEVERITY_ORDER: Record<RiskStatusLevel, number> = { Critical: 2, Caution: 1, Stable: 0 };

/** GET /doctors/me/patients?status=&search=&page= (only THIS doctor's patients, most severe first). */
function getMyPatients(ctx: ProtectedContext): DemoResponse {
  const result = currentDoctor(ctx);
  if ('error' in result) return result.error;

  const filter = STATUS_FILTERS.find(
    (candidate) => candidate === (ctx.request.query.status ?? 'all'),
  );
  if (!filter) {
    return problem(400, 'Validation failed', undefined, { status: ['Invalid status filter.'] });
  }

  const search = (ctx.request.query.search ?? '').trim().toLowerCase();
  const requestedPage = Number.parseInt(ctx.request.query.page ?? '1', 10);
  const language = currentLanguage();

  const items = patientsOfDoctor(ctx.db, result.doctor.id)
    .filter((patient) => matchesFilter(statusLevelOf(patient) ?? 'Stable', filter))
    .filter(
      (patient) =>
        search === '' ||
        patient.fullName.toLowerCase().includes(search) ||
        patient.nationalId.includes(search) ||
        patient.fileNumber.toLowerCase().includes(search),
    )
    .map((patient) => buildListItem(patient, ctx.request.nowMs, language))
    .sort(
      (a, b) =>
        SEVERITY_ORDER[b.statusLevel] - SEVERITY_ORDER[a.statusLevel] ||
        a.fullName.localeCompare(b.fullName),
    );

  const totalPages = Math.max(1, Math.ceil(items.length / DEMO_PAGE_SIZE));
  const page = Math.min(Math.max(1, Number.isNaN(requestedPage) ? 1 : requestedPage), totalPages);
  const body: PaginatedResponse<PatientListItem> = {
    items: items.slice((page - 1) * DEMO_PAGE_SIZE, page * DEMO_PAGE_SIZE),
    page,
    pageSize: DEMO_PAGE_SIZE,
    totalCount: items.length,
    totalPages,
  };
  return json(200, body);
}

/** GET /doctors/me/ward-stats */
function getWardStats(ctx: ProtectedContext): DemoResponse {
  const result = currentDoctor(ctx);
  if ('error' in result) return result.error;
  return json(200, buildWardStats(ctx.db, result.doctor.id, ctx.request.nowMs));
}

/** POST /doctors/me/patients: links an existing patient by national ID (idempotent). */
function addPatient(ctx: ProtectedContext): DemoResponse {
  const result = currentDoctor(ctx);
  if ('error' in result) return result.error;

  const identifier = asText(asRecord(ctx.request.body).identifier);
  if (!identifier) {
    return problem(400, 'Validation failed', undefined, { identifier: ['Required.'] });
  }

  const patient = ctx.db.patients.find((candidate) => candidate.nationalId === identifier);
  if (!patient) return problem(404, 'Not Found', 'No patient with this national ID.');

  let status = 200;
  if (!isAssigned(ctx.db, result.doctor.id, patient.id)) {
    const hasPrimary = ctx.db.assignments.some((a) => a.patientId === patient.id && a.isPrimary);
    ctx.db.assignments.push({
      doctorId: result.doctor.id,
      patientId: patient.id,
      assignedAt: new Date(ctx.request.nowMs).toISOString(),
      isPrimary: !hasPrimary,
    });
    saveDemoDb(ctx.db);
    status = 201;
  }
  return json(status, buildListItem(patient, ctx.request.nowMs, currentLanguage()));
}

// ---------------------------------------------------------------------------
// Device endpoints
// ---------------------------------------------------------------------------

/** POST /devices/pair: a patient pairs their own device; a doctor must name an assigned patient. */
function pairDevice(ctx: ProtectedContext): DemoResponse {
  const body = asRecord(ctx.request.body);
  const serial = asText(body.serialNumber);
  if (!serial) return problem(400, 'Validation failed', undefined, { serialNumber: ['Required.'] });

  const target = ctx.auth.claims.role === 'Patient' ? 'me' : asText(body.patientId);
  if (!target) {
    return problem(400, 'Validation failed', undefined, { patientId: ['Required for doctors.'] });
  }
  const lookup = resolvePatient(ctx, target);
  if ('error' in lookup) return lookup.error;

  const outcome = pairDeviceToPatient(ctx.db, lookup.patient, serial, ctx.request.nowMs);
  if (!outcome.ok) {
    if (outcome.reason === 'invalidSerial') {
      return problem(400, 'Validation failed', undefined, {
        serialNumber: ['Use letters, digits and dashes (4-40 characters).'],
      });
    }
    return problem(
      409,
      'Conflict',
      outcome.reason === 'deviceTaken'
        ? 'This device is already paired to another patient.'
        : 'This patient already has a paired device.',
    );
  }
  saveDemoDb(ctx.db);
  return json(200, buildDeviceSummary(outcome.device, lookup.patient, ctx.request.nowMs));
}

/** GET /devices/{serial}/status. An inaccessible device answers 404 (not 403) so serial numbers cannot be probed. */
function getDeviceStatus(ctx: ProtectedContext): DemoResponse {
  const serial = param(ctx, 'serial').toUpperCase();
  const device = ctx.db.devices.find((candidate) => candidate.serialNumber === serial);
  const notFound = problem(404, 'Not Found', 'Device not found.');
  if (!device?.patientId || !canAccessPatient(ctx.db, ctx.auth, device.patientId)) return notFound;

  const patient = ctx.db.patients.find((candidate) => candidate.id === device.patientId);
  return patient ? json(200, buildDeviceSummary(device, patient, ctx.request.nowMs)) : notFound;
}

// ---------------------------------------------------------------------------
// Route table and dispatcher
// ---------------------------------------------------------------------------

/** Turns "/patients/:target/alerts" into a matcher plus the ordered parameter names. */
function compile(path: string): { matcher: RegExp; names: string[] } {
  const names: string[] = [];
  const pattern = path.replace(/:([A-Za-z]+)/g, (_match, name: string) => {
    names.push(name);
    return '([^/]+)';
  });
  return { matcher: new RegExp(`^${pattern}$`), names };
}

function publicRoute(
  method: DemoMethod,
  path: string,
  handler: (ctx: BaseContext) => MaybeAsync<DemoResponse>,
): Route {
  return { kind: 'public', method, handler, ...compile(path) };
}

function protectedRoute(
  method: DemoMethod,
  path: string,
  handler: (ctx: ProtectedContext) => MaybeAsync<DemoResponse>,
): Route {
  return { kind: 'protected', method, handler, ...compile(path) };
}

const ROUTES: readonly Route[] = [
  publicRoute('POST', '/auth/login', login),
  publicRoute('POST', '/auth/signup/patient', signupPatient),
  publicRoute('POST', '/auth/signup/doctor', signupDoctor),
  publicRoute('POST', '/auth/refresh', refresh),
  publicRoute('POST', '/auth/logout', logout),

  protectedRoute('GET', '/patients/:target', getPatient),
  protectedRoute('PUT', '/patients/me', updateMyPatientProfile),
  protectedRoute('GET', '/patients/:target/vitals-history', getVitalsHistory),
  protectedRoute('GET', '/patients/:target/alerts', getAlerts),
  protectedRoute('GET', '/patients/:target/ai-report', getAiReport),
  protectedRoute('POST', '/patients/:target/manual-events', logManualEvent),
  protectedRoute('POST', '/patients/:target/freeze-waveform', freezeWaveform),

  protectedRoute('GET', '/doctors/me', getDoctorMe),
  protectedRoute('PUT', '/doctors/me', updateDoctorMe),
  protectedRoute('GET', '/doctors/me/patients', getMyPatients),
  protectedRoute('POST', '/doctors/me/patients', addPatient),
  protectedRoute('GET', '/doctors/me/ward-stats', getWardStats),

  protectedRoute('POST', '/devices/pair', pairDevice),
  protectedRoute('GET', '/devices/:serial/status', getDeviceStatus),
];

/** Extracts decoded path parameters for a matched route. */
function extractParams(route: Route, path: string): Record<string, string> {
  const match = route.matcher.exec(path);
  const params: Record<string, string> = {};
  route.names.forEach((name, index) => {
    const raw = match?.[index + 1] ?? '';
    try {
      params[name] = decodeURIComponent(raw);
    } catch {
      params[name] = raw;
    }
  });
  return params;
}

/** Entry point used by the Axios adapter: routes one request and always resolves with a response. */
export async function handleDemoRequest(request: DemoRequest): Promise<DemoResponse> {
  const path = request.path.replace(/\/+$/, '') || '/';
  const pathMatches = ROUTES.filter((route) => route.matcher.test(path));
  if (pathMatches.length === 0) {
    return problem(
      404,
      'Not Found',
      `The demo backend has no endpoint for ${request.method} ${path}.`,
    );
  }
  const route = pathMatches.find((candidate) => candidate.method === request.method);
  if (!route) return problem(405, 'Method Not Allowed');

  try {
    const db = loadDemoDb(request.nowMs);
    const params = extractParams(route, path);
    if (route.kind === 'public') return await route.handler({ request, params, db });

    const authentication = authenticate(request, db);
    if (!authentication.ok) return authentication.response;
    return await route.handler({ request, params, db, auth: authentication.auth });
  } catch (error) {
    console.error('Demo backend error:', error);
    return problem(500, 'Internal Server Error', 'The demo backend hit an unexpected error.');
  }
}
