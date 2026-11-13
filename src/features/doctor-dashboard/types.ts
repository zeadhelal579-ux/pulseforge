import type { PatientListStatusFilter } from '@/shared/types/api';

/** Matches the ?status=&search=&page= query params on GET /doctors/me/patients. */
export interface PatientListQueryParams {
  status: PatientListStatusFilter;
  search: string;
  page: number;
}

export const PATIENT_LIST_PAGE_SIZE = 5;
