import { useTranslation } from 'react-i18next';
import { Phone, RefreshCw } from 'lucide-react';
import { Button, buttonVariants } from '@/shared/components/ui/Button';
import { usePatientProfile } from '@/features/patient-dashboard/hooks/usePatientProfile';
import { usePatientAlerts } from '@/features/patient-dashboard/hooks/usePatientAlerts';
import { usePatientVitals } from '@/features/patient-dashboard/hooks/usePatientVitals';
import { PatientDetailContent } from '@/features/patient-dashboard/components/PatientDetailContent';

export function PatientDashboardPage() {
  const { t } = useTranslation();
  const profileQuery = usePatientProfile();
  const alertsQuery = usePatientAlerts();

  const vitalsState = usePatientVitals(profileQuery.data?.id, {
    vitals: profileQuery.data?.latestVitals ?? null,
    riskAssessment: profileQuery.data?.latestRiskAssessment ?? null,
  });

  if (profileQuery.isLoading) {
    return <p className="p-6 text-center text-text-muted">{t('common.loading')}</p>;
  }

  if (profileQuery.isError || !profileQuery.data) {
    return (
      <div className="flex flex-col items-center gap-3 p-10 text-center">
        <p className="text-text-muted">{t('common.error.generic')}</p>
        <Button variant="secondary" onClick={() => profileQuery.refetch()}>
          {t('common.retry')}
        </Button>
      </div>
    );
  }

  const profile = profileQuery.data;
  const assignedDoctor = profile.assignedDoctor;

  return (
    <PatientDetailContent
      profile={profile}
      vitalsState={vitalsState}
      alerts={alertsQuery.data ?? []}
      alertsLoading={alertsQuery.isLoading}
      showNumericScore={false}
      headerActions={
        <>
          <Button variant="secondary" size="sm" onClick={() => profileQuery.refetch()}>
            <RefreshCw className="size-3.5" />
            {t('common.refresh')}
          </Button>
          {/* Per PROJECT_OVERVIEW.md §3: the Call button rings the assigned
              doctor and disappears entirely when none is assigned yet. A
              plain tel: link needs no dedicated telephony backend endpoint. */}
          {assignedDoctor ? (
            <a
              href={`tel:${assignedDoctor.phone}`}
              className={buttonVariants({ variant: 'secondary', size: 'sm' })}
            >
              <Phone className="size-3.5" />
              {assignedDoctor.fullName}
            </a>
          ) : (
            <p className="self-center text-xs text-text-muted">
              {t('patient.callDoctor.noDoctorAssigned')}
            </p>
          )}
        </>
      }
    />
  );
}
