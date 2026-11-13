import { useTranslation } from 'react-i18next';
import { Heart, Droplets } from 'lucide-react';
import { Card } from '@/shared/components/ui/Card';
import { formatBpm, formatPercent } from '@/shared/utils/formatters';

interface VitalsCardProps {
  heartRate: number | null;
  spo2: number | null;
  className?: string;
}

/**
 * Coarse, non-diagnostic reference-range labels for the sublabel under each
 * vital. These replace a previous version that always showed a static
 * "Sinus Rhythm" / "Excellent Perfusion" string regardless of the actual
 * reading — misleading if a value were ever abnormal, and at odds with this
 * project's own "never falsely reassure" principle (PROJECT_OVERVIEW.md §4).
 *
 * ASSUMPTION: thresholds are commonly-cited adult reference ranges (resting
 * HR 60–100 bpm; SpO2 ≥95% typical, 90–94% below typical, <90% notably low),
 * not a clinical determination — deliberately phrased as "range" language
 * rather than a diagnostic term (e.g. not "Bradycardia"/"Tachycardia"), and
 * should be confirmed against real clinical guidance before this ships.
 */
function heartRateRangeKey(value: number | null): string {
  if (value === null) return 'common.empty.noData';
  if (value < 60) return 'patient.vitals.belowRange';
  if (value > 100) return 'patient.vitals.aboveRange';
  return 'patient.vitals.withinRange';
}

function spo2RangeKey(value: number | null): string {
  if (value === null) return 'common.empty.noData';
  if (value < 90) return 'patient.vitals.notablyLow';
  if (value < 95) return 'patient.vitals.belowRange';
  return 'patient.vitals.withinRange';
}

/** One tile — heart rate or SpO2 — kept generic enough to reuse for either. */
function VitalTile({
  icon: Icon,
  label,
  value,
  sublabel,
}: {
  icon: typeof Heart;
  label: string;
  value: string;
  sublabel: string;
}) {
  return (
    <Card>
      <div className="mb-2 flex items-center gap-1.5 text-xs font-semibold text-text-muted">
        <Icon className="size-3.5" />
        {label}
      </div>
      <p className="font-mono text-3xl font-bold text-text-primary">{value}</p>
      <p className="mt-1 text-xs text-text-muted">{sublabel}</p>
    </Card>
  );
}

export function VitalsCard({ heartRate, spo2, className }: VitalsCardProps) {
  const { t, i18n } = useTranslation();
  const locale = i18n.language as 'ar' | 'en';

  return (
    <div className={className}>
      <div className="grid grid-cols-2 gap-3">
        <VitalTile
          icon={Heart}
          label={t('patient.vitals.heartRate')}
          value={heartRate !== null ? formatBpm(heartRate, locale) : '—'}
          sublabel={t(heartRateRangeKey(heartRate))}
        />
        <VitalTile
          icon={Droplets}
          label={t('patient.vitals.spo2')}
          value={spo2 !== null ? formatPercent(spo2, locale) : '—'}
          sublabel={t(spo2RangeKey(spo2))}
        />
      </div>
    </div>
  );
}
