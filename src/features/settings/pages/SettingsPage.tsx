import { useTranslation } from 'react-i18next';
import { Moon, User } from 'lucide-react';
import { Card, CardHeader, CardTitle } from '@/shared/components/ui/Card';
import { ThemeToggle } from '@/shared/components/ThemeToggle';
import { useAuthStore } from '@/features/auth/store/authStore';
import { LanguagePreference } from '@/features/settings/components/LanguagePreference';
import { DevicePairingForm } from '@/features/settings/components/DevicePairingForm';
import { usePatientProfile } from '@/features/patient-dashboard/hooks/usePatientProfile';
import { isPatient } from '@/shared/utils/guards';

export function SettingsPage() {
  const { t } = useTranslation();
  const user = useAuthStore((state) => state.user);
  const showDevicePairing = isPatient(user);

  // Only fetched for a patient — a doctor has no device of their own to
  // pair. React Query's `enabled` flag stops this from firing an unnecessary
  // /patients/me request (and a 403) when the logged-in user is a doctor.
  const profileQuery = usePatientProfile(undefined, showDevicePairing);

  return (
    <div className="mx-auto flex max-w-2xl flex-col gap-4">
      <h1 className="text-lg font-bold text-text-primary">{t('settings.title')}</h1>

      <LanguagePreference />

      <Card>
        <CardHeader>
          <CardTitle className="flex items-center gap-1.5 text-sm normal-case text-text-primary">
            <Moon className="size-4 text-accent" />
            {t('settings.themePreference.title')}
          </CardTitle>
        </CardHeader>
        <p className="mb-4 text-sm text-text-secondary">
          {t('settings.themePreference.description')}
        </p>
        <ThemeToggle />
      </Card>

      {showDevicePairing && <DevicePairingForm device={profileQuery.data?.device} />}

      <Card>
        <CardHeader>
          <CardTitle className="flex items-center gap-1.5 text-sm normal-case text-text-primary">
            <User className="size-4 text-accent" />
            {t('settings.account.title')}
          </CardTitle>
        </CardHeader>
        <dl className="grid grid-cols-1 gap-3 text-sm sm:grid-cols-2">
          <div>
            <dt className="text-xs text-text-muted">{t('auth.fields.fullName')}</dt>
            <dd className="text-text-primary">{user?.fullName}</dd>
          </div>
          <div>
            <dt className="text-xs text-text-muted">{t('auth.fields.email')}</dt>
            <dd className="text-text-primary">{user?.email}</dd>
          </div>
        </dl>
      </Card>
    </div>
  );
}
