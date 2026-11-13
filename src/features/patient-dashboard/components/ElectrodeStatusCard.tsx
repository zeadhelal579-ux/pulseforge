import { useTranslation } from 'react-i18next';
import { ShieldAlert, ShieldCheck } from 'lucide-react';
import { Card, CardHeader, CardTitle } from '@/shared/components/ui/Card';
import { formatPercent } from '@/shared/utils/formatters';
import type { ElectrodeVerification } from '@/shared/types/api';

interface ElectrodeStatusCardProps {
  verification: ElectrodeVerification | null;
  className?: string;
}

export function ElectrodeStatusCard({ verification, className }: ElectrodeStatusCardProps) {
  const { t, i18n } = useTranslation();
  const locale = i18n.language as 'ar' | 'en';
  const needsReposition = verification?.placementStatus === 'Reposition Needed';

  return (
    <Card className={className}>
      <CardHeader>
        <CardTitle>{t('patient.electrode.title')}</CardTitle>
        {verification && (
          <span className="font-mono text-xs font-bold text-accent">
            {formatPercent(verification.confidenceScore * 100, locale)}{' '}
            {t('patient.electrode.efficiency')}
          </span>
        )}
      </CardHeader>

      {verification ? (
        <div className="flex items-start gap-3 rounded-lg bg-surface-alt p-3">
          {needsReposition
            ? <ShieldAlert className="mt-0.5 size-5 shrink-0 text-warning" />
            : <ShieldCheck className="mt-0.5 size-5 shrink-0 text-accent" />}
          <div>
            <p className={needsReposition ? 'text-sm font-semibold text-warning' : 'text-sm font-semibold text-accent'}>
              {verification.placementStatus}
            </p>
            <p className="mt-1 text-xs text-text-muted">
              {t(needsReposition ? 'patient.electrode.reposition' : 'patient.electrode.contactImpedance')}
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
