import { apiClient } from '@/shared/lib/apiClient';
import type { AiReport, AiReportPeriod, Alert, PatientProfile, VitalsSample } from '@/shared/types/api';
import type { VitalsHistoryRange } from '@/features/patient-dashboard/types';

/** Builds "/patients/me" or "/patients/{id}" depending on who's asking. */
function patientBasePath(patientId?: string): string {
  return patientId ? `/patients/${patientId}` : '/patients/me';
}

/**
 * GET /api/patients/me | GET /api/patients/{id}
 * The full aggregate the whole detail page renders from (see the PatientProfile
 * doc-comment in shared/types/api.ts for why this is one call, not several).
 */
export async function getPatientProfile(patientId?: string): Promise<PatientProfile> {
  const { data } = await apiClient.get<PatientProfile>(patientBasePath(patientId));
  return data;
}

/** PUT /api/patients/me — patients can only ever update their own profile. */
export async function updateMyProfile(payload: Partial<PatientProfile>): Promise<PatientProfile> {
  const { data } = await apiClient.put<PatientProfile>('/patients/me', payload);
  return data;
}

/** GET /api/patients/me/vitals-history?range= | GET /api/patients/{id}/vitals-history?range= */
export async function getPatientVitalsHistory(
  range: VitalsHistoryRange,
  patientId?: string,
): Promise<VitalsSample[]> {
  const { data } = await apiClient.get<VitalsSample[]>(
    `${patientBasePath(patientId)}/vitals-history`,
    { params: { range } },
  );
  return data;
}

/** GET /api/patients/me/alerts | GET /api/patients/{id}/alerts */
export async function getPatientAlerts(patientId?: string): Promise<Alert[]> {
  const { data } = await apiClient.get<Alert[]>(`${patientBasePath(patientId)}/alerts`);
  return data;
}

/**
 * GET /api/patients/{id|me}/ai-report?period=
 * See the "ASSUMPTION / GAP" note above `AiReport` in shared/types/api.ts —
 * this endpoint doesn't exist in the backend doc yet and needs to be added.
 */
export async function getPatientAiReport(
  period: AiReportPeriod,
  patientId?: string,
): Promise<AiReport> {
  const { data } = await apiClient.get<AiReport>(`${patientBasePath(patientId)}/ai-report`, {
    params: { period },
  });
  return data;
}
