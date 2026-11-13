import { useState } from 'react';
import { useForm } from 'react-hook-form';
import { useMutation, useQueryClient } from '@tanstack/react-query';
import { useTranslation } from 'react-i18next';
import { UserPlus, Users } from 'lucide-react';
import { Button } from '@/shared/components/ui/Button';
import { Modal } from '@/shared/components/ui/Modal';
import { Input } from '@/shared/components/ui/Input';
import { WardStatsCards } from '@/features/doctor-dashboard/components/WardStatsCards';
import { PatientListFilters } from '@/features/doctor-dashboard/components/PatientListFilters';
import { PatientListItem } from '@/features/doctor-dashboard/components/PatientListItem';
import { usePatientList } from '@/features/doctor-dashboard/hooks/usePatientList';
import { useWardStats } from '@/features/doctor-dashboard/hooks/useWardStats';
import { addPatient } from '@/features/doctor-dashboard/api/doctorApi';

interface AddPatientFormValues {
  identifier: string;
}

function AddPatientModal({ isOpen, onClose }: { isOpen: boolean; onClose: () => void }) {
  const { t } = useTranslation();
  const queryClient = useQueryClient();
  const { register, handleSubmit, reset } = useForm<AddPatientFormValues>();

  const mutation = useMutation({
    mutationFn: (values: AddPatientFormValues) => addPatient(values),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['patients-list'] });
      queryClient.invalidateQueries({ queryKey: ['ward-stats'] });
      reset();
      onClose();
    },
  });

  return (
    <Modal isOpen={isOpen} onClose={onClose} title={t('doctor.addNewPatient')}>
      <form onSubmit={handleSubmit((values) => mutation.mutate(values))} noValidate>
        <Input
          label={t('auth.fields.nationalId')}
          placeholder={t('auth.fields.nationalIdPh')}
          required
          {...register('identifier', { required: true })}
        />
        {mutation.isError && (
          <p className="mb-3 text-xs text-danger">{t('common.error.generic')}</p>
        )}
        <div className="flex justify-end gap-2">
          <Button type="button" variant="secondary" onClick={onClose}>
            {t('common.cancel')}
          </Button>
          <Button type="submit" isLoading={mutation.isPending}>
            {t('common.confirm')}
          </Button>
        </div>
      </form>
    </Modal>
  );
}

export function PatientListPage() {
  const { t } = useTranslation();
  const [isAddPatientOpen, setAddPatientOpen] = useState(false);
  const wardStatsQuery = useWardStats();
  const { search, setSearch, status, setStatus, page, setPage, pageSize, data, isLoading } =
    usePatientList();

  const stats = wardStatsQuery.data;
  const counts = {
    all: stats?.activePatients ?? 0,
    stable: stats?.stableCount ?? 0,
    alert: stats?.cautionCount ?? 0,
    critical: stats?.criticalCasesCount ?? 0,
  };

  const totalPages = data?.totalPages ?? 1;
  const totalCount = data?.totalCount ?? 0;
  const rangeStart = totalCount === 0 ? 0 : (page - 1) * pageSize + 1;
  const rangeEnd = Math.min(page * pageSize, totalCount);

  return (
    <div className="flex flex-col gap-4">
      <div className="flex items-center justify-between">
        <h1 className="text-lg font-bold text-text-primary">{t('nav.patients')}</h1>
        <Button size="sm" onClick={() => setAddPatientOpen(true)}>
          <UserPlus className="size-4" />
          {t('doctor.addNewPatient')}
        </Button>
      </div>

      {stats && <WardStatsCards stats={stats} />}

      <PatientListFilters
        search={search}
        onSearchChange={setSearch}
        status={status}
        onStatusChange={setStatus}
        counts={counts}
      />

      {isLoading ? (
        <p className="p-6 text-center text-text-muted">{t('common.loading')}</p>
      ) : !data || data.items.length === 0 ? (
        <div className="flex flex-col items-center gap-2 rounded-xl border border-dashed border-border p-10 text-center">
          <Users className="size-8 text-text-muted" />
          <p className="font-semibold text-text-primary">{t('doctor.patientList.empty.title')}</p>
          <p className="text-sm text-text-muted">{t('doctor.patientList.empty.description')}</p>
        </div>
      ) : (
        <div className="flex flex-col gap-3">
          {data.items.map((patient) => (
            <PatientListItem key={patient.id} patient={patient} />
          ))}
        </div>
      )}

      {data && data.items.length > 0 && (
        <div className="flex flex-wrap items-center justify-between gap-3 text-sm text-text-muted">
          <p>
            {t('doctor.pagination.showing')} {rangeStart}-{rangeEnd} {t('doctor.pagination.of')}{' '}
            {totalCount} {t('doctor.pagination.patients')}
          </p>
          <div className="flex gap-2">
            <Button
              variant="secondary"
              size="sm"
              disabled={page <= 1}
              onClick={() => setPage((current) => Math.max(1, current - 1))}
            >
              {t('doctor.pagination.previous')}
            </Button>
            <Button
              variant="secondary"
              size="sm"
              disabled={page >= totalPages}
              onClick={() => setPage((current) => Math.min(totalPages, current + 1))}
            >
              {t('doctor.pagination.next')}
            </Button>
          </div>
        </div>
      )}

      <AddPatientModal isOpen={isAddPatientOpen} onClose={() => setAddPatientOpen(false)} />
    </div>
  );
}
