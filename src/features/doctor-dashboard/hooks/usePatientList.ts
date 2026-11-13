import { useEffect, useState } from 'react';
import { useQuery } from '@tanstack/react-query';
import { useDebouncedValue } from '@/shared/hooks/useDebouncedValue';
import { getMyPatients } from '@/features/doctor-dashboard/api/doctorApi';
import { PATIENT_LIST_PAGE_SIZE } from '@/features/doctor-dashboard/types';
import type { PatientListStatusFilter } from '@/shared/types/api';

export function usePatientList() {
  const [search, setSearch] = useState('');
  const [status, setStatus] = useState<PatientListStatusFilter>('all');
  const [page, setPage] = useState(1);
  const debouncedSearch = useDebouncedValue(search, 300);

  // Any filter change invalidates the current page number.
  useEffect(() => {
    setPage(1);
  }, [debouncedSearch, status]);

  const query = useQuery({
    queryKey: ['patients-list', { status, search: debouncedSearch, page }],
    queryFn: () => getMyPatients({ status, search: debouncedSearch, page }),
    placeholderData: (previousData) => previousData, // avoid a loading flash while paging/filtering
  });

  return {
    search,
    setSearch,
    status,
    setStatus,
    page,
    setPage,
    pageSize: PATIENT_LIST_PAGE_SIZE,
    ...query,
  };
}
