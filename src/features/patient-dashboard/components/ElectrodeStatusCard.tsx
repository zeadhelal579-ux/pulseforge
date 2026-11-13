import { useTranslation } from 'react-i18next';
import { ShieldAlert, ShieldCheck } from 'lucide-react';
import { Card, CardHeader, CardTitle } from '@/shared/components/ui/Card';
import { cn } from '@/shared/utils/cn';
import { formatPercent } from '@/shared/utils/formatters';
import type { ElectrodeVerification } from '@/shared/types/api';

interface ElectrodeStatusCardProps {
  verification: ElectrodeVerification | null;
  className?: string;
}

/**
 * The backend owns the exact placement strings (ElectrodePlacementStatus: "Optimal Placement" /
 * "Reposition Needed"). Anything mentioning "reposition" is the non-optimal state, which must never be
 * presented with the reassuring "fully secure" copy (project rule: never falsely reassure).
 */
const REPOSITION_STATUS = /reposition/i;

export function ElectrodeStatusCard({ verification, className }: ElectrodeStatusCardProps) {
  const { t, i18n } = useTranslation();
  const locale = i18n.language as 'ar' | 'en';
  const needsRepositioning = verification
    ? REPOSITION_STATUS.test(verification.placementStatus)
    : false;
  const toneClass = needsRepositioning ? 'text-warning' : 'text-accent';
  const StatusIcon = needsRepositioning ? ShieldAlert : ShieldCheck;

  return (
    <Card className={className}>
      <CardHeader>
        <CardTitle>{t('patient.electrode.title')}</CardTitle>
        {verification && (
          <span className={cn('font-mono text-xs font-bold', toneClass)}>
            {formatPercent(verification.confidenceScore * 100, locale)}{' '}
            {t('patient.electrode.efficiency')}
          </span>
        )}
      </CardHeader>

      {verification ? (
        <div className="flex items-start gap-3 rounded-lg bg-surface-alt p-3">
          <StatusIcon className={cn('mt-0.5 size-5 shrink-0', toneClass)} />
          <div>
            <p className={cn('text-sm font-semibold', toneClass)}>{verification.placementStatus}</p>
            <p className="mt-1 text-xs text-text-muted">
              {needsRepositioning
                ? t('patient.electrode.repositionHint')
                : t('patient.electrode.contactImpedance')}
            </p>
            {verification.impedanceKOhm && (
              <div className="mt-2 flex flex-wrap gap-2 font-mono text-[11px] text-text-secondary">
                {Object.entries(verification.impedanceKOhm).map(([lead, value]) => (
                  <span key={lead} className="rounded bg-surface px-1.5 py-0.5">
                    {lead}: {value.toFixed(1)} kΩ
                  </span>
                ))}
              </div>
            )}
          </div>
        </div>
      ) : (
        <p className="text-sm text-text-muted">{t('common.empty.noData')}</p>
      )}
    </Card>
  );
}
