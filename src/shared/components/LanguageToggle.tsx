import { useTranslation } from 'react-i18next';
import { cn } from '@/shared/utils/cn';

const LANGUAGES = [
  { code: 'ar', label: 'AR' },
  { code: 'en', label: 'EN' },
] as const;

interface LanguageToggleProps {
  className?: string;
}

export function LanguageToggle({ className }: LanguageToggleProps) {
  const { t, i18n } = useTranslation();

  return (
    <div
      className={cn('flex rounded-full border border-border bg-surface-alt p-[3px]', className)}
      role="group"
      aria-label={t('common.language')}
    >
      {LANGUAGES.map((language) => {
        const isActive = i18n.language === language.code;
        return (
          <button
            key={language.code}
            type="button"
            onClick={() => i18n.changeLanguage(language.code)}
            aria-pressed={isActive}
            className={cn(
              'rounded-full px-[15px] py-1.5 font-mono text-[12.5px] font-bold transition-colors',
              isActive ? 'bg-accent-subtle text-accent' : 'text-text-muted hover:text-text-secondary',
            )}
          >
            {language.label}
          </button>
        );
      })}
    </div>
  );
}
