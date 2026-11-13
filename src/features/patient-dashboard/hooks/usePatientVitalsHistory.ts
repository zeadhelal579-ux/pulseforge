import { useQuery } from '@tanstack/react-query';
import { getPatientVitalsHistory } from '@/features/patient-dashboard/api/patientApi';
import type { VitalsHistoryRange } from '@/features/patient-dashboard/types';

export function usePatientVitalsHistory(range: VitalsHistoryRange, patientId?: string) {
  return useQuery({
    queryKey: ['patient-vitals-history', patientId ?? 'me', range],
    queryFn: () => getPatientVitalsHistory(range, patientId),
    staleTime: 60_000,
  });
}
