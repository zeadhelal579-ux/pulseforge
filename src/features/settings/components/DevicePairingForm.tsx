import { useForm } from 'react-hook-form';
import { useMutation, useQueryClient } from '@tanstack/react-query';
import { useTranslation } from 'react-i18next';
import { Cpu } from 'lucide-react';
import { Card, CardHeader, CardTitle } from '@/shared/components/ui/Card';
import { Input } from '@/shared/components/ui/Input';
import { Button } from '@/shared/components/ui/Button';
import { pairDevice } from '@/features/settings/api/settingsApi';
import type { DeviceSummary } from '@/shared/types/api';

interface DevicePairingFormValues {
  serialNumber: string;
}

interface DevicePairingFormProps {
  device: DeviceSummary | null | undefined;
}

export function DevicePairingForm({ device }: DevicePairingFormProps) {
  const { t } = useTranslation();
  const queryClient = useQueryClient();
  const { register, handleSubmit, reset } = useForm<DevicePairingFormValues>();

  const mutation = useMutation({
    mutationFn: (values: DevicePairingFormValues) =>
      pairDevice({ serialNumber: values.serialNumber }),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['patient-profile', 'me'] });
      reset();
    },
  });

  return (
    <Card>
      <CardHeader>
        <CardTitle className="flex items-center gap-1.5 text-sm normal-case text-text-primary">
          <Cpu className="size-4 text-accent" />
          {t('settings.devicePairing.title')}
        </CardTitle>
      </CardHeader>
      <p className="mb-4 text-sm text-text-secondary">{t('settings.devicePairing.description')}</p>

      {device ? (
        <div className="flex items-center justify-between rounded-lg bg-surface-alt p-3">
          <div>
            <p className="text-xs text-text-muted">{t('settings.devicePairing.pairedDevice')}</p>
            <p className="font-mono text-sm font-semibold text-text-primary">
              {device.serialNumber}
            </p>
          </div>
          <span className="font-mono text-xs text-text-muted">
            {t('patient.medicalFile.battery')}: {device.batteryLevel}%
          </span>
        </div>
      ) : (
        <form
          onSubmit={handleSubmit((values) => mutation.mutate(values))}
          className="flex items-end gap-3"
          noValidate
        >
          <Input
            label={t('settings.devicePairing.serialNumber')}
            placeholder={t('settings.devicePairing.serialNumberPh')}
            required
            className="mb-0"
            {...register('serialNumber', { required: true })}
          />
          <Button type="submit" isLoading={mutation.isPending} className="mb-4">
            {t('settings.devicePairing.pairButton')}
          </Button>
        </form>
      )}
      {mutation.isError && <p className="mt-2 text-xs text-danger">{t('common.error.generic')}</p>}
    </Card>
  );
}
