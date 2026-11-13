import type { ReactNode } from 'react';
import { useTranslation } from 'react-i18next';
import { Watch } from 'lucide-react';
import { StatusPill } from '@/shared/components/StatusPill';
import { VitalsCard } from '@/features/patient-dashboard/components/VitalsCard';
import { RiskStatusCard } from '@/features/patient-dashboard/components/RiskStatusCard';
import { EcgWaveform } from '@/features/patient-dashboard/components/EcgWaveform';
import { HourlyStatusTimeline } from '@/features/patient-dashboard/components/HourlyStatusTimeline';
import { VitalsTrendChart } from '@/features/patient-dashboard/components/VitalsTrendChart';
import { ElectrodeStatusCard } from '@/features/patient-dashboard/components/ElectrodeStatusCard';
import { MedicalFileCard } from '@/features/patient-dashboard/components/MedicalFileCard';
import { AlertsTimeline } from '@/features/patient-dashboard/components/AlertsTimeline';
import { AiReportCard } from '@/features/patient-dashboard/components/AiReportCard';
import type { Alert, PatientProfile } from '@/shared/types/api';
import type { LiveVitalsState } from '@/features/patient-dashboard/hooks/usePatientVitals';

interface PatientDetailContentProps {
  /** undefined on the patient's own dashboard ("me"); set on the doctor's detail view. */
  patientId?: string;
  profile: PatientProfile;
  vitalsState: LiveVitalsState;
  alerts: Alert[];
  alertsLoading: boolean;
  /** Doctor-only — see RiskStatusCard's own doc-comment for why this is a prop, not a role check. */
  showNumericScore: boolean;
  /** Top-right action buttons — differ by role (patient: "Call my doctor";
   *  doctor: "Call patient" / "Send message" / "Refresh"), supplied by the caller. */
  headerActions?: ReactNode;
  /** ECG card controls — only ever supplied by the doctor's PatientDetailChrome. */
  isWaveformFrozen?: boolean;
  onToggleFreezeWaveform?: () => void;
  onReviewStrip?: () => void;
  onLogManualEvent?: () => void;
}

export function PatientDetailContent({
  patientId,
  profile,
  vitalsState,
  alerts,
  alertsLoading,
  showNumericScore,
  headerActions,
  isWaveformFrozen,
  onToggleFreezeWaveform,
  onReviewStrip,
  onLogManualEvent,
}: PatientDetailContentProps) {
  const { t } = useTranslation();
  const hasAnyData = profile.latestVitals !== null || profile.latestRiskAssessment !== null;

  return (
    <div className="flex flex-col gap-4">
      {/* Header */}
      <div className="flex flex-wrap items-start justify-between gap-4 rounded-xl border border-border bg-surface p-4">
        <div>
          <div className="mb-1 flex items-center gap-2">
            <h1 className="text-lg font-bold text-text-primary">{profile.fullName}</h1>
            <span className="text-sm text-text-muted">{profile.age}</span>
            {vitalsState.statusLevel && <StatusPill status={vitalsState.statusLevel} />}
          </div>
          <p className="text-sm text-text-secondary">
            {t('patient.diagnosisLabel')}: {profile.diagnosis}
          </p>
        </div>
        {headerActions && <div className="flex flex-wrap gap-2">{headerActions}</div>}
      </div>

      {!hasAnyData && (
        <div className="flex items-center gap-3 rounded-xl border border-dashed border-border bg-surface p-4">
          <Watch className="size-6 shrink-0 text-text-muted" />
          <div>
            <p className="text-sm font-semibold text-text-primary">{t('patient.empty.title')}</p>
            <p className="text-xs text-text-muted">{t('patient.empty.description')}</p>
          </div>
        </div>
      )}

      {/* Top row: vitals + risk status */}
      <div className="grid grid-cols-1 gap-4 lg:grid-cols-3">
        <VitalsCard
          heartRate={vitalsState.heartRate}
          spo2={vitalsState.spo2}
          className="lg:col-span-2"
        />
        <RiskStatusCard
          statusLevel={vitalsState.statusLevel}
          riskScore={vitalsState.riskScore}
          showNumericScore={showNumericScore}
        />
      </div>

      {/* Live ECG */}
      <EcgWaveform
        patientId={patientId ?? profile.id}
        ecgFeatureSet={profile.latestEcgFeatureSet}
        isFrozen={isWaveformFrozen}
        onToggleFreeze={onToggleFreezeWaveform}
        onReviewStrip={onReviewStrip}
        onLogManualEvent={onLogManualEvent}
      />

      <div className="grid grid-cols-1 gap-4 lg:grid-cols-2">
        <HourlyStatusTimeline buckets={profile.hourlyStatusTimeline} />
        <VitalsTrendChart patientId={patientId} />
      </div>

      <div className="grid grid-cols-1 gap-4 lg:grid-cols-2">
        <ElectrodeStatusCard verification={profile.latestElectrodeVerification} />
        <MedicalFileCard medicalHistory={profile.medicalHistory} device={profile.device} />
      </div>

      <AlertsTimeline alerts={alerts} isLoading={alertsLoading} />

      <AiReportCard patientId={patientId} patientName={profile.fullName} />
    </div>
  );
}
