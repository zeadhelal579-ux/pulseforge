import { Link } from 'react-router-dom';
import { useTranslation } from 'react-i18next';
import { FolderOpen, Phone, Radio } from 'lucide-react';
import { Card } from '@/shared/components/ui/Card';
import { Button, buttonVariants } from '@/shared/components/ui/Button';
import { StatusPill } from '@/shared/components/StatusPill';
import { formatRelativeTime, formatBpm, formatPercent } from '@/shared/utils/formatters';
import { cn } from '@/shared/utils/cn';
import type { PatientListItem as PatientListItemDto, RiskStatusLevel } from '@/shared/types/api';

interface PatientListItemProps {
  patient: PatientListItemDto;
}

const STATUS_BORDER: Record<RiskStatusLevel, string> = {
  Stable: 'border-s-accent',
  Caution: 'border-s-warning',
  Critical: 'border-s-danger',
};

export function PatientListItem({ patient }: PatientListItemProps) {
  const { t, i18n } = useTranslation();
  const locale = i18n.language as 'ar' | 'en';

  return (
    <Card
      className={cn(
        'flex flex-col gap-4 border-s-4 md:flex-row md:items-center md:justify-between',
        STATUS_BORDER[patient.statusLevel],
      )}
    >
      <div className="min-w-0 flex-1">
        <div className="mb-1 flex flex-wrap items-center gap-2">
          <h3 className="font-semibold text-text-primary">{patient.fullName}</h3>
          <span className="text-xs text-text-muted">({patient.age})</span>
          <StatusPill status={patient.statusLevel} label={patient.statusLabel} />
          {patient.isLive && (
            <span className="flex items-center gap-1 text-[11px] font-semibold text-accent">
              <Radio className="size-3 animate-pulse" />
              {t('doctor.patientList.liveNow')}
            </span>
          )}
        </div>
        <p className="text-xs text-text-muted">
          {patient.diagnosis} · {formatRelativeTime(patient.lastUpdatedAt, locale)}
        </p>
        <p className="mt-1 font-mono text-[11px] text-text-muted">
          {t('doctor.patientList.file')}: #{patient.fileNumber}
          {patient.bedOrRoom && ` · ${patient.bedOrRoom}`}
        </p>
      </div>

      <div className="flex items-center gap-4 rounded-lg bg-surface-alt px-4 py-2 text-sm">
        <div>
          <p className="text-[10px] uppercase text-text-muted">HR</p>
          <p className="font-mono font-bold text-text-primary">{formatBpm(patient.heartRate, locale)}</p>
        </div>
        <div>
          <p className="text-[10px] uppercase text-text-muted">SpO2</p>
          <p className="font-mono font-bold text-text-primary">{formatPercent(patient.spo2, locale)}</p>
        </div>
      </div>

      <div className="flex shrink-0 gap-2">
        <Link
          to={`/doctor/patients/${patient.id}`}
          className={buttonVariants({ variant: 'secondary', size: 'sm' })}
        >
          <FolderOpen className="size-3.5" />
          {t('doctor.patientList.openFullFile')}
        </Link>
        {patient.statusLevel === 'Critical' ? (
          <Button variant="danger" size="sm">
            <Phone className="size-3.5" />
            {t('doctor.patientList.urgentCall')}
          </Button>
        ) : (
          <Button variant="secondary" size="sm">
            <Phone className="size-3.5" />
          </Button>
        )}
      </div>
    </Card>
  );
}
