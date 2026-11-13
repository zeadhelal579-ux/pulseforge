import { useTranslation } from 'react-i18next';
import { SegmentedControl } from '@/shared/components/ui/SegmentedControl';
import type { SignupRole } from '@/features/auth/types';

interface RoleToggleProps {
  value: SignupRole;
  onChange: (role: SignupRole) => void;
}

export function RoleToggle({ value, onChange }: RoleToggleProps) {
  const { t } = useTranslation();

  return (
    <SegmentedControl<SignupRole>
      aria-label={`${t('auth.rolePatient')} / ${t('auth.roleDoctor')}`}
      value={value}
      onChange={onChange}
      className="mb-6"
      options={[
        { value: 'patient', label: t('auth.rolePatient') },
        { value: 'doctor', label: t('auth.roleDoctor') },
      ]}
    />
  );
}
