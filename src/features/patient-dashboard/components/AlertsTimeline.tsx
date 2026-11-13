import { useTranslation } from 'react-i18next';
import { AlertTriangle, Bell, CheckCircle2 } from 'lucide-react';
import { Card, CardHeader, CardTitle } from '@/shared/components/ui/Card';
import { Badge } from '@/shared/components/ui/Badge';
import { formatRelativeTime } from '@/shared/utils/formatters';
import { cn } from '@/shared/utils/cn';
import type { Alert, AlertSeverity } from '@/shared/types/api';

interface AlertsTimelineProps {
  alerts: Alert[];
  isLoading?: boolean;
  className?: string;
}

const SEVERITY_BORDER: Record<AlertSeverity, string> = {
  Info: 'border-s-accent',
  Transient: 'border-s-accent',
  Moderate: 'border-s-warning',
  Critical: 'border-s-danger',
};

const SEVERITY_BADGE_STATUS: Record<AlertSeverity, 'stable' | 'caution' | 'critical'> = {
  Info: 'stable',
  Transient: 'stable',
  Moderate: 'caution',
  Critical: 'critical',
};

function SeverityIcon({ severity }: { severity: AlertSeverity }) {
  if (severity === 'Critical') return <AlertTriangle className="size-4 text-danger" />;
  if (severity === 'Moderate') return <Bell className="size-4 text-warning" />;
  return <CheckCircle2 className="size-4 text-accent" />;
}

export function AlertsTimeline({ alerts, isLoading, className }: AlertsTimelineProps) {
  const { t, i18n } = useTranslation();
  const locale = i18n.language as 'ar' | 'en';

  return (
    <Card className={className}>
      <CardHeader>
        <CardTitle>{t('patient.alertsTimeline.title')}</CardTitle>
        <span className="text-xs text-text-muted">
          {alerts.length} {t('patient.alertsTimeline.recordedEvents')}
        </span>
      </CardHeader>

      {isLoading ? (
        <p className="text-sm text-text-muted">{t('common.loading')}</p>
      ) : alerts.length === 0 ? (
        <p className="text-sm text-text-muted">{t('patient.alertsTimeline.noAlerts')}</p>
      ) : (
        <ul className="flex flex-col gap-3">
          {alerts.map((alert) => (
            <li
              key={alert.id}
              className={cn(
                'flex items-start gap-3 rounded-lg border-s-4 bg-surface-alt p-3',
                SEVERITY_BORDER[alert.severity],
              )}
            >
              <SeverityIcon severity={alert.severity} />
              <div className="min-w-0 flex-1">
                <div className="flex flex-wrap items-center justify-between gap-2">
                  <p className="text-sm font-semibold text-text-primary">{alert.message}</p>
                  <span className="whitespace-nowrap text-xs text-text-muted">
                    {formatRelativeTime(alert.triggeredAt, locale)}
                  </span>
                </div>
                <Badge status={SEVERITY_BADGE_STATUS[alert.severity]} className="mt-1.5">
                  {alert.type}
                </Badge>
                {alert.resolutionNote && (
                  <p className="mt-1.5 text-xs text-text-muted">{alert.resolutionNote}</p>
                )}
              </div>
            </li>
          ))}
        </ul>
      )}
    </Card>
  );
}
