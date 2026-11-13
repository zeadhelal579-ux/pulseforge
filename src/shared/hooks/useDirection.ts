import { useTranslation } from 'react-i18next';

export type TextDirection = 'rtl' | 'ltr';

const RTL_LANGUAGES = new Set(['ar']);

/**
 * Returns 'rtl' | 'ltr' for the currently active language. I18nProvider is
 * what actually *applies* this to <html dir="...">; this hook is for
 * components that need to branch on direction directly (e.g. choosing which
 * side an icon sits on) rather than relying on Tailwind logical properties
 * alone.
 */
export function useDirection(): TextDirection {
  const { i18n } = useTranslation();
  return RTL_LANGUAGES.has(i18n.language) ? 'rtl' : 'ltr';
}
