import { useTranslation } from 'react-i18next';
import { AlertOctagon, Radio, ShieldAlert, Users } from 'lucide-react';
import { Card } from '@/shared/components/ui/Card';
import { cn } from '@/shared/utils/cn';
import type { WardStats } from '@/shared/types/api';

interface WardStatsCardsProps {
  stats: WardStats;
  className?: string;
}

interface StatTileProps {
  icon: typeof Users;
  label: string;
  value: string;
  detail: string;
  progressPercent: number;
  accentClassName: string;
}

function StatTile({ icon: Icon, label, value, detail, progressPercent, accentClassName }: StatTileProps) {
  return (
    <Card>
      <div className="mb-2 flex items-center justify-between">
        <span className="text-xs font-semibold uppercase tracking-wide text-text-muted">
          {label}
        </span>
        <Icon className={cn('size-4', accentClassName)} />
      </div>
      <p className="font-mono text-2xl font-bold text-text-primary">{value}</p>
      <p className="mt-1 text-xs text-text-muted">{detail}</p>
      <div className="mt-2 h-1 w-full overflow-hidden rounded-full bg-surface-alt">
        <div
          className={cn('h-full rounded-full', accentClassName.replace('text-', 'bg-'))}
          style={{ width: `${Math.min(100, Math.max(0, progressPercent))}%` }}
        />
      </div>
    </Card>
  );
}

export function WardStatsCards({ stats, className }: WardStatsCardsProps) {
  const { t } = useTranslation();

  const wardOccupancyPercent =
    stats.wardCapacity > 0 ? Math.round((stats.activePatients / stats.wardCapacity) * 100) : 0;
  const criticalPercent =
    stats.criticalCapacityTotal > 0
      ? Math.round((stats.criticalCasesCount / stats.criticalCapacityTotal) * 100)
      : 0;
  const deviceLinkPercent = stats.deviceLinkRatePercent;

  return (
    <div className={cn('grid grid-cols-1 gap-3 sm:grid-cols-2 lg:grid-cols-4', className)}>
      <StatTile
        icon={Users}
        label={t('doctor.wardStats.totalActivePatients')}
        value={`${stats.activePatients}/${stats.wardCapacity}`}
        detail={`${wardOccupancyPercent}% ${t('doctor.wardStats.wardCapacity')}`}
        progressPercent={wardOccupancyPercent}
        accentClassName="text-accent"
      />
      <StatTile
        icon={AlertOctagon}
        label={t('doctor.wardStats.criticalCases')}
        value={String(stats.criticalCasesCount).padStart(2, '0')}
        detail={
          stats.criticalCasesCount > 0
            ? t('doctor.wardStats.requiresImmediate')
            : t('common.status.Stable')
        }
        progressPercent={criticalPercent}
        accentClassName="text-danger"
      />
      <StatTile
        icon={Radio}
        label={t('doctor.wardStats.connectedDevices')}
        value={`${stats.connectedDevices}/${stats.totalDevices}`}
        detail={`${stats.totalDevices - stats.connectedDevices} ${t('doctor.wardStats.devicesPending')}`}
        progressPercent={deviceLinkPercent}
        accentClassName="text-accent"
      />
      <StatTile
        icon={ShieldAlert}
        label={t('doctor.wardStats.signalAccuracy')}
        value={`${stats.signalAccuracyPercent.toFixed(1)}%`}
        detail={t('doctor.wardStats.optimalInterference')}
        progressPercent={stats.signalAccuracyPercent}
        accentClassName="text-accent"
      />
    </div>
  );
}
