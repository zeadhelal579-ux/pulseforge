import { apiClient } from '@/shared/lib/apiClient';
import type {
  AddPatientRequest,
  DoctorProfile,
  FreezeWaveformRequest,
  LogManualEventRequest,
  Alert,
  PaginatedResponse,
  PatientListItem,
  WardStats,
} from '@/shared/types/api';
import type { PatientListQueryParams } from '@/features/doctor-dashboard/types';

/**
 * GET /api/doctors/me — ASSUMPTION / GAP: backend-database-architecture.md §5's
 * table lists `GET /api/doctors/me/patients` and `GET /api/doctors/me/ward-stats`
 * but no plain `GET /api/doctors/me` to fetch the doctor's own profile (name,
 * specialty, hospital — shown in the dashboard header). Added here by symmetry
 * with `GET /api/patients/me`; needs to exist on the backend.
 */
export async function getDoctorProfile(): Promise<DoctorProfile> {
  const { data } = await apiClient.get<DoctorProfile>('/doctors/me');
  return data;
}

/**
 * PUT /api/doctors/me — same gap as above (no doctor equivalent of
 * `PUT /api/patients/me` is listed). Needed so a doctor's language preference
 * can be persisted the same way frontend-architecture.md §9 describes for patients.
 */
export async function updateMyDoctorProfile(
  payload: Partial<DoctorProfile>,
): Promise<DoctorProfile> {
  const { data } = await apiClient.put<DoctorProfile>('/doctors/me', payload);
  return data;
}

/** GET /api/doctors/me/patients?status=&search=&page= */
export async function getMyPatients(
  params: PatientListQueryParams,
): Promise<PaginatedResponse<PatientListItem>> {
  const { data } = await apiClient.get<PaginatedResponse<PatientListItem>>('/doctors/me/patients', {
    params,
  });
  return data;
}

/** GET /api/doctors/me/ward-stats */
export async function getWardStats(): Promise<WardStats> {
  const { data } = await apiClient.get<WardStats>('/doctors/me/ward-stats');
  return data;
}

/** POST /api/doctors/me/patients — links an existing patient by national ID or invite code. */
export async function addPatient(payload: AddPatientRequest): Promise<PatientListItem> {
  const { data } = await apiClient.post<PatientListItem>('/doctors/me/patients', payload);
  return data;
}

/** POST /api/patients/{id}/manual-events — "Log Manual Event" doctor-only control. */
export async function logManualEvent(
  patientId: string,
  payload: LogManualEventRequest,
): Promise<Alert> {
  const { data } = await apiClient.post<Alert>(`/patients/${patientId}/manual-events`, payload);
  return data;
}

/**
 * POST /api/patients/{id}/freeze-waveform — "Freeze Waveform" doctor-only control.
 * Per backend-database-architecture.md §6, raw ECG isn't persisted to SQL, so
 * this almost certainly just toggles an ephemeral session flag rather than
 * writing a row — confirm the exact semantics with backend before relying on
 * its response body for anything beyond an acknowledgement.
 */
export async function setWaveformFrozen(
  patientId: string,
  payload: FreezeWaveformRequest,
): Promise<void> {
  await apiClient.post(`/patients/${patientId}/freeze-waveform`, payload);
}
