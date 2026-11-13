import { useQuery } from '@tanstack/react-query';
import { getPatientAlerts } from '@/features/patient-dashboard/api/patientApi';

export function usePatientAlerts(patientId?: string) {
  return useQuery({
    queryKey: ['patient-alerts', patientId ?? 'me'],
    queryFn: () => getPatientAlerts(patientId),
  });
}
