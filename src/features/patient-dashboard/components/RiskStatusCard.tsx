import { useTranslation } from 'react-i18next';
import { Card } from '@/shared/components/ui/Card';
import { StatusPill } from '@/shared/components/StatusPill';
import type { RiskStatusLevel } from '@/shared/types/api';
import { cn } from '@/shared/utils/cn';

interface RiskStatusCardProps {
  statusLevel: RiskStatusLevel | null;
  riskScore?: number;
  /** True only for the doctor's view — a patient must never see the raw
   *  number (backend-database-architecture.md §6 / PROJECT_OVERVIEW.md §4). */
  showNumericScore: boolean;
  className?: string;
}

const STATUS_BAR_COLOR: Record<RiskStatusLevel, string> = {
  Stable: 'bg-accent',
  Caution: 'bg-warning',
  Critical: 'bg-danger',
};

export function RiskStatusCard({
  statusLevel,
  riskScore,
  showNumericScore,
  className,
}: RiskStatusCardProps) {
  const { t } = useTranslation();

  return (
    <Card className={className}>
      <div className="mb-2 flex items-center justify-between">
        <span className="text-xs font-semibold text-text-muted">
          {t('patient.vitals.riskScore')}
        </span>
        {statusLevel && <StatusPill status={statusLevel} />}
      </div>

      {showNumericScore && typeof riskScore === 'number' ? (
        <>
          <p className="font-mono text-3xl font-bold text-text-primary">
            {riskScore}
            <span className="text-base font-normal text-text-muted">/100</span>
          </p>
          <div className="mt-2 h-1.5 w-full overflow-hidden rounded-full bg-surface-alt">
            <div
              className={cn('h-full rounded-full', statusLevel && STATUS_BAR_COLOR[statusLevel])}
              style={{ width: `${Math.min(100, Math.max(0, riskScore))}%` }}
            />
          </div>
          <p className="mt-1 text-xs text-text-muted">{t('doctor.chrome.numericRiskScore')}</p>
        </>
      ) : (
        <p className="text-sm text-text-secondary">
          {statusLevel ? t(`common.status.${statusLevel}`) : t('common.empty.noData')}
        </p>
      )}
    </Card>
  );
}
