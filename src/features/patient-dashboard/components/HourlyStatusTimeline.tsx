import { useTranslation } from 'react-i18next';
import { Card, CardHeader, CardTitle } from '@/shared/components/ui/Card';
import { formatHour } from '@/shared/utils/formatters';
import { cn } from '@/shared/utils/cn';
import type { HourlyStatusBucket, RiskStatusLevel } from '@/shared/types/api';

interface HourlyStatusTimelineProps {
  buckets: HourlyStatusBucket[];
  className?: string;
}

const STATUS_BLOCK_COLOR: Record<RiskStatusLevel, string> = {
  Stable: 'bg-accent/70',
  Caution: 'bg-warning/70',
  Critical: 'bg-danger/70',
};

export function HourlyStatusTimeline({ buckets, className }: HourlyStatusTimelineProps) {
  const { t, i18n } = useTranslation();
  const locale = i18n.language as 'ar' | 'en';

  const stableHours = buckets.filter((bucket) => bucket.status === 'Stable').length;
  const alertHours = buckets.filter((bucket) => bucket.status === 'Caution').length;
  const criticalHours = buckets.filter((bucket) => bucket.status === 'Critical').length;
  const hasCurrentHour = buckets.some((bucket) => bucket.isCurrent);
  const coveragePercent = buckets.length > 0 ? Math.round((buckets.length / 24) * 100) : 0;

  return (
    <Card className={className}>
      <CardHeader>
        <CardTitle>{t('patient.hourlyTimeline.title')}</CardTitle>
        <span className="text-xs text-text-muted">{t('patient.hourlyTimeline.last24h')}</span>
      </CardHeader>

      {buckets.length === 0 ? (
        <p className="text-sm text-text-muted">{t('common.empty.noData')}</p>
      ) : (
        <>
          <div className="mb-2 flex gap-1">
            {buckets.map((bucket) => (
              <div
                key={bucket.hour}
                title={formatHour(bucket.hour, locale)}
                className={cn(
                  'h-8 flex-1 rounded-sm',
                  STATUS_BLOCK_COLOR[bucket.status],
                  bucket.isCurrent && 'ring-2 ring-accent ring-offset-1 ring-offset-surface',
                )}
              />
            ))}
          </div>
          <div className="mb-3 flex justify-between font-mono text-[10px] text-text-muted">
            <span>{formatHour(buckets[0]?.hour ?? 0, locale)}</span>
            <span>{formatHour(buckets[buckets.length - 1]?.hour ?? 23, locale)}</span>
          </div>
          <div className="flex flex-wrap items-center gap-x-4 gap-y-1 text-xs text-text-secondary">
            <LegendDot colorClass="bg-accent/70" label={`${t('patient.hourlyTimeline.stable')}: ${stableHours}`} />
            <LegendDot
              colorClass="bg-warning/70"
              label={`${t('patient.hourlyTimeline.cautionAlert')}: ${alertHours}`}
            />
            <LegendDot colorClass="bg-danger/70" label={`${t('patient.hourlyTimeline.critical')}: ${criticalHours}`} />
            {hasCurrentHour && (
              <LegendDot
                colorClass="bg-accent ring-2 ring-accent ring-offset-1 ring-offset-surface"
                label={t('patient.hourlyTimeline.currentHour')}
              />
            )}
            <span className="ms-auto text-text-muted">
              {t('patient.hourlyTimeline.totalCoverage')}: {coveragePercent}%
            </span>
          </div>
        </>
      )}
    </Card>
  );
}

function LegendDot({ colorClass, label }: { colorClass: string; label: string }) {
  return (
    <span className="flex items-center gap-1.5">
      <span className={cn('size-2 rounded-full', colorClass)} />
      {label}
    </span>
  );
}
