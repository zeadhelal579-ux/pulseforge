import { useTranslation } from 'react-i18next';
import { Search } from 'lucide-react';
import { SegmentedControl } from '@/shared/components/ui/SegmentedControl';
import type { PatientListStatusFilter } from '@/shared/types/api';

interface PatientListFiltersProps {
  search: string;
  onSearchChange: (value: string) => void;
  status: PatientListStatusFilter;
  onStatusChange: (value: PatientListStatusFilter) => void;
  counts: { all: number; stable: number; alert: number; critical: number };
}

export function PatientListFilters({
  search,
  onSearchChange,
  status,
  onStatusChange,
  counts,
}: PatientListFiltersProps) {
  const { t } = useTranslation();

  return (
    <div className="mb-4 flex flex-col gap-3 md:flex-row md:items-center">
      <div className="relative flex-1">
        <Search className="pointer-events-none absolute start-3.5 top-1/2 size-4 -translate-y-1/2 text-text-muted" />
        <input
          type="search"
          aria-label={t('doctor.search.placeholder')}
          value={search}
          onChange={(event) => onSearchChange(event.target.value)}
          placeholder={t('doctor.search.placeholder')}
          className="w-full rounded-xl border border-border bg-surface-alt py-2.5 ps-10 pe-3.5 text-sm text-text-primary placeholder:text-text-muted focus:border-accent focus:outline-none"
        />
      </div>

      <SegmentedControl<PatientListStatusFilter>
        aria-label={t('doctor.search.placeholder')}
        value={status}
        onChange={onStatusChange}
        className="md:w-auto"
        options={[
          { value: 'all', label: t('doctor.filters.all'), count: counts.all },
          { value: 'stable', label: t('doctor.filters.stable'), count: counts.stable },
          { value: 'alert', label: t('doctor.filters.alert'), count: counts.alert },
          { value: 'critical', label: t('doctor.filters.critical'), count: counts.critical },
        ]}
      />
    </div>
  );
}
