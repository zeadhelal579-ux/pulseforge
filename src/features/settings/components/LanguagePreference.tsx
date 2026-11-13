import { useTranslation } from 'react-i18next';
import { Languages } from 'lucide-react';
import { Card, CardHeader, CardTitle } from '@/shared/components/ui/Card';
import { LanguageToggle } from '@/shared/components/LanguageToggle';

export function LanguagePreference() {
  const { t } = useTranslation();

  return (
    <Card>
      <CardHeader>
        <CardTitle className="flex items-center gap-1.5 text-sm normal-case text-text-primary">
          <Languages className="size-4 text-accent" />
          {t('settings.languagePreference.title')}
        </CardTitle>
      </CardHeader>
      <p className="mb-4 text-sm text-text-secondary">
        {t('settings.languagePreference.description')}
      </p>
      <LanguageToggle />
    </Card>
  );
}
