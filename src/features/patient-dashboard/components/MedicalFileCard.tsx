import { useTranslation } from 'react-i18next';
import { FileText } from 'lucide-react';
import { Card, CardHeader, CardTitle } from '@/shared/components/ui/Card';
import { formatSignalStrength } from '@/shared/utils/formatters';
import type { DeviceSummary, MedicalHistoryEntry } from '@/shared/types/api';

interface MedicalFileCardProps {
  medicalHistory: MedicalHistoryEntry[];
  device: DeviceSummary | null | undefined;
  className?: string;
}

function groupByType(entries: MedicalHistoryEntry[]) {
  const diagnoses = entries.filter((entry) => entry.entryType !== 'Medication');
  const medications = entries.filter((entry) => entry.entryType === 'Medication');
  return { diagnoses, medications };
}

export function MedicalFileCard({ medicalHistory, device, className }: MedicalFileCardProps) {
  const { t } = useTranslation();
  const { diagnoses, medications } = groupByType(medicalHistory);

  return (
    <Card className={className}>
      <CardHeader>
        <CardTitle className="flex items-center gap-1.5">
          <FileText className="size-3.5" />
          {t('patient.medicalFile.title')}
        </CardTitle>
        <span className="text-[11px] text-text-muted">{t('patient.medicalFile.autoUpdated')}</span>
      </CardHeader>

      <dl className="grid grid-cols-1 gap-3 text-sm sm:grid-cols-2">
        <div>
          <dt className="text-xs text-text-muted">{t('patient.medicalFile.medicalHistory')}</dt>
          <dd className="mt-0.5 text-text-primary">
            {diagnoses.length > 0
              ? diagnoses.map((entry) => entry.description).join(', ')
              : t('common.empty.noData')}
          </dd>
        </div>
        <div>
          <dt className="text-xs text-text-muted">{t('patient.medicalFile.activeMedications')}</dt>
          <dd className="mt-0.5 text-text-primary">
            {medications.length > 0
              ? medications.map((entry) => entry.description).join(', ')
              : t('common.empty.noData')}
          </dd>
        </div>

        {device && (
          <>
            <div>
              <dt className="text-xs text-text-muted">{t('patient.medicalFile.serialNo')}</dt>
              <dd className="mt-0.5 font-mono text-text-primary">{device.serialNumber}</dd>
            </div>
            <div>
              <dt className="text-xs text-text-muted">{t('patient.medicalFile.battery')}</dt>
              <dd className="mt-0.5 font-mono text-text-primary">{device.batteryLevel}%</dd>
            </div>
            <div className="sm:col-span-2">
              <dt className="text-xs text-text-muted">
                {t('patient.medicalFile.transmissionProtocol')}
              </dt>
              <dd className="mt-0.5 font-mono text-text-primary">
                {device.connectionMode}
                {typeof device.signalStrengthDbm === 'number' &&
                  ` (${formatSignalStrength(device.signalStrengthDbm)})`}
              </dd>
            </div>
          </>
        )}
      </dl>
    </Card>
  );
}
