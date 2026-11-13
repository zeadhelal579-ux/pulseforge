import { useQuery } from '@tanstack/react-query';
import { getPatientProfile } from '@/features/patient-dashboard/api/patientApi';

export function usePatientProfile(patientId?: string, enabled = true) {
  return useQuery({
    queryKey: ['patient-profile', patientId ?? 'me'],
    queryFn: () => getPatientProfile(patientId),
    enabled,
    // Vitals-adjacent fields go stale fast, but SignalR (not refetching)
    // keeps them current after the initial load — see usePatientVitals.
    staleTime: 15_000,
  });
}
