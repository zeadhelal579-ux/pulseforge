import { useQuery } from '@tanstack/react-query';
import { getWardStats } from '@/features/doctor-dashboard/api/doctorApi';

export function useWardStats() {
  return useQuery({
    queryKey: ['ward-stats'],
    queryFn: getWardStats,
    staleTime: 60_000,
  });
}
