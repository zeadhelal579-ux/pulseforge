import { useEffect, type ReactNode } from 'react';
import { I18nextProvider, useTranslation } from 'react-i18next';
import { i18n } from '@/app/providers/i18n';

const RTL_LANGUAGES = new Set(['ar']);

function DocumentAttributesSync() {
  const { i18n: i18nInstance } = useTranslation();

  useEffect(() => {
    const applyToDocument = (language: string) => {
      const dir = RTL_LANGUAGES.has(language) ? 'rtl' : 'ltr';
      document.documentElement.setAttribute('lang', language);
      document.documentElement.setAttribute('dir', dir);
      document.body.setAttribute('data-lang', language);
    };

    applyToDocument(i18nInstance.language);
    i18nInstance.on('languageChanged', applyToDocument);
    return () => i18nInstance.off('languageChanged', applyToDocument);
  }, [i18nInstance]);

  return null;
}

interface I18nProviderProps {
  children: ReactNode;
}

export function I18nProvider({ children }: I18nProviderProps) {
  return (
    <I18nextProvider i18n={i18n}>
      <DocumentAttributesSync />
      {children}
    </I18nextProvider>
  );
}
