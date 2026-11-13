import { useParams } from 'react-router-dom';
import { useTranslation } from 'react-i18next';
import { Button } from '@/shared/components/ui/Button';
import { useJoinPatientGroup } from '@/shared/hooks/useJoinPatientGroup';
import { usePatientProfile } from '@/features/patient-dashboard/hooks/usePatientProfile';
import { usePatientAlerts } from '@/features/patient-dashboard/hooks/usePatientAlerts';
import { usePatientVitals } from '@/features/patient-dashboard/hooks/usePatientVitals';
import { PatientDetailChrome } from '@/features/doctor-dashboard/components/PatientDetailChrome';

export function PatientDetailPage() {
  const { t } = useTranslation();
  const { patientId } = useParams<{ patientId: string }>();

  const profileQuery = usePatientProfile(patientId);
  const alertsQuery = usePatientAlerts(patientId);

  const vitalsState = usePatientVitals(patientId, {
    vitals: profileQuery.data?.latestVitals ?? null,
    riskAssessment: profileQuery.data?.latestRiskAssessment ?? null,
  });

  // Per backend-database-architecture.md §5: a doctor's connection joins a
  // patient's SignalR group only while actively viewing that patient's
  // detail page — not all of their assigned patients' groups at once.
  useJoinPatientGroup(patientId, true);

  if (!patientId) {
    return <p className="p-6 text-center text-text-muted">{t('common.error.notFound')}</p>;
  }

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

  return (
    <PatientDetailChrome
      // Forces React to unmount/remount the whole detail-view subtree when
      // the doctor moves from one patient's URL directly to another's
      // (browser back/forward, or an edited URL) without an intervening
      // trip back to the patient list. Without this, PatientDetailChrome's
      // own local state (e.g. `isFrozen`) and EcgWaveform's live ring
      // buffer would otherwise persist across the switch and briefly show
      // one patient's frozen/live waveform state as if it belonged to the
      // next patient — see useLiveEcgStream.ts for the matching
      // defense-in-depth reset.
      key={patientId}
      patientId={patientId}
      profile={profileQuery.data}
      vitalsState={vitalsState}
      alerts={alertsQuery.data ?? []}
      alertsLoading={alertsQuery.isLoading}
      onRefetch={() => profileQuery.refetch()}
    />
  );
}
