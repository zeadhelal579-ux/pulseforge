import { useTranslation } from 'react-i18next';
import { Badge } from '@/shared/components/ui/Badge';
import type { RiskStatusLevel } from '@/shared/types/api';

const STATUS_TO_BADGE_VARIANT = {
  Stable: 'stable',
  Caution: 'caution',
  Critical: 'critical',
} as const;

interface StatusPillProps {
  status: RiskStatusLevel;
  /** Overrides the default translated label — used for backend-provided
   *  descriptive labels like "Fully Stable" or "Technical Alert" on the
   *  doctor's patient list (see PatientListItem.statusLabel). */
  label?: string;
  className?: string;
}

export function StatusPill({ status, label, className }: StatusPillProps) {
  const { t } = useTranslation();

  return (
    <Badge status={STATUS_TO_BADGE_VARIANT[status]} withDot className={className}>
      {label ?? t(`common.status.${status}`)}
    </Badge>
  );
}
