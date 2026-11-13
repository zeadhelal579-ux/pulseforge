import { useState } from 'react';
import { useForm } from 'react-hook-form';
import { useMutation, useQueryClient } from '@tanstack/react-query';
import { useTranslation } from 'react-i18next';
import { Mail, Phone, RefreshCw } from 'lucide-react';
import { Button, buttonVariants } from '@/shared/components/ui/Button';
import { Modal } from '@/shared/components/ui/Modal';
import { Textarea } from '@/shared/components/ui/Textarea';
import { Select } from '@/shared/components/ui/Select';
import { PatientDetailContent } from '@/features/patient-dashboard/components/PatientDetailContent';
import { logManualEvent, setWaveformFrozen } from '@/features/doctor-dashboard/api/doctorApi';
import type { LiveVitalsState } from '@/features/patient-dashboard/hooks/usePatientVitals';
import type { Alert, AlertSeverity, PatientProfile } from '@/shared/types/api';

interface ManualEventFormValues {
  note: string;
  severity: AlertSeverity;
}

function ManualEventModal({
  isOpen,
  onClose,
  patientId,
}: {
  isOpen: boolean;
  onClose: () => void;
  patientId: string;
}) {
  const { t } = useTranslation();
  const queryClient = useQueryClient();
  const { register, handleSubmit, reset } = useForm<ManualEventFormValues>({
    defaultValues: { severity: 'Moderate' },
  });

  const mutation = useMutation({
    mutationFn: (values: ManualEventFormValues) => logManualEvent(patientId, values),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['patient-alerts', patientId] });
      reset();
      onClose();
    },
  });

  return (
    <Modal isOpen={isOpen} onClose={onClose} title={t('doctor.manualEvent.title')}>
      <form onSubmit={handleSubmit((values) => mutation.mutate(values))} noValidate>
        <Textarea
          label={t('doctor.manualEvent.noteLabel')}
          placeholder={t('doctor.manualEvent.notePlaceholder')}
          required
          {...register('note', { required: true })}
        />
        <Select
          label={t('doctor.manualEvent.severityLabel')}
          options={[
            { value: 'Info', label: t('doctor.manualEvent.severityInfo') },
            { value: 'Transient', label: t('doctor.manualEvent.severityTransient') },
            { value: 'Moderate', label: t('doctor.manualEvent.severityModerate') },
            { value: 'Critical', label: t('doctor.manualEvent.severityCritical') },
          ]}
          {...register('severity')}
        />
        <div className="mt-2 flex justify-end gap-2">
          <Button type="button" variant="secondary" onClick={onClose}>
            {t('common.cancel')}
          </Button>
          <Button type="submit" isLoading={mutation.isPending}>
            {t('doctor.manualEvent.submit')}
          </Button>
        </div>
      </form>
    </Modal>
  );
}

interface PatientDetailChromeProps {
  patientId: string;
  profile: PatientProfile;
  vitalsState: LiveVitalsState;
  alerts: Alert[];
  alertsLoading: boolean;
  onRefetch: () => void;
}

export function PatientDetailChrome({
  patientId,
  profile,
  vitalsState,
  alerts,
  alertsLoading,
  onRefetch,
}: PatientDetailChromeProps) {
  const { t } = useTranslation();
  const [isFrozen, setIsFrozen] = useState(false);
  const [isManualEventOpen, setManualEventOpen] = useState(false);
  const [isReviewStripOpen, setReviewStripOpen] = useState(false);

  const freezeMutation = useMutation({
    mutationFn: (frozen: boolean) => setWaveformFrozen(patientId, { frozen }),
  });

  const handleToggleFreeze = () => {
    const next = !isFrozen;
    setIsFrozen(next); // update the canvas immediately — don't wait on the network round trip
    freezeMutation.mutate(next);
  };

  return (
    <>
      <PatientDetailContent
        patientId={patientId}
        profile={profile}
        vitalsState={vitalsState}
        alerts={alerts}
        alertsLoading={alertsLoading}
        showNumericScore
        isWaveformFrozen={isFrozen}
        onToggleFreezeWaveform={handleToggleFreeze}
        onReviewStrip={() => setReviewStripOpen(true)}
        onLogManualEvent={() => setManualEventOpen(true)}
        headerActions={
          <>
            <Button variant="secondary" size="sm" onClick={onRefetch}>
              <RefreshCw className="size-3.5" />
              {t('common.refresh')}
            </Button>
            <a
              href={`tel:${profile.phone}`}
              className={buttonVariants({ variant: 'secondary', size: 'sm' })}
            >
              <Phone className="size-3.5" />
              {t('doctor.patientList.call')}
            </a>
            <a
              href={`mailto:${profile.email}`}
              className={buttonVariants({ variant: 'secondary', size: 'sm' })}
            >
              <Mail className="size-3.5" />
              {t('doctor.patientList.sendMessage')}
            </a>
          </>
        }
      />

      <ManualEventModal
        isOpen={isManualEventOpen}
        onClose={() => setManualEventOpen(false)}
        patientId={patientId}
      />

      <Modal
        isOpen={isReviewStripOpen}
        onClose={() => setReviewStripOpen(false)}
        title={t('patient.ecg.reviewStrip')}
      >
        <p className="text-sm leading-relaxed text-text-secondary">
          {t('doctor.reviewStripUnavailable')}
        </p>
      </Modal>
    </>
  );
}
