import { Moon, Sun } from 'lucide-react';
import { useTranslation } from 'react-i18next';
import { applyThemeToDocument, useThemeStore } from '@/shared/store/themeStore';
import { cn } from '@/shared/utils/cn';

interface ThemeToggleProps {
  className?: string;
}

export function ThemeToggle({ className }: ThemeToggleProps) {
  const { t } = useTranslation();
  const mode = useThemeStore((state) => state.mode);
  const toggleTheme = useThemeStore((state) => state.toggleTheme);

  const handleClick = () => {
    toggleTheme();
    // Read the *next* value straight from the store rather than the stale
    // `mode` closed over by this render, so the DOM attribute is correct
    // even if this handler fires twice in quick succession.
    applyThemeToDocument(useThemeStore.getState().mode);
  };

  const isDark = mode === 'dark';

  return (
    <button
      type="button"
      onClick={handleClick}
      aria-label={isDark ? t('common.theme.switchToLight') : t('common.theme.switchToDark')}
      title={isDark ? t('common.theme.switchToLight') : t('common.theme.switchToDark')}
      className={cn(
        'flex size-9 items-center justify-center rounded-full border border-border bg-surface-alt',
        'text-text-secondary transition-colors hover:text-accent',
        className,
      )}
    >
      {isDark ? <Moon className="size-[18px]" /> : <Sun className="size-[18px]" />}
    </button>
  );
}
