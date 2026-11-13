import i18next from 'i18next';
import { initReactI18next } from 'react-i18next';
import LanguageDetector from 'i18next-browser-languagedetector';
import en from '@/locales/en/translation.json';
import ar from '@/locales/ar/translation.json';

export const i18n = i18next.createInstance();

void i18n
  .use(LanguageDetector)
  .use(initReactI18next)
  .init({
    resources: {
      en: { translation: en },
      ar: { translation: ar },
    },
    fallbackLng: (import.meta.env.VITE_DEFAULT_LANGUAGE as string) ?? 'ar',
    supportedLngs: ['ar', 'en'],
    detection: {
      // Checked in order: a saved choice always wins over the browser's
      // reported language, matching "language preference... follows the
      // user" from frontend-architecture.md §9.
      order: ['localStorage', 'navigator'],
      caches: ['localStorage'],
      lookupLocalStorage: 'cardiosense-language',
    },
    interpolation: {
      escapeValue: false, // React already escapes output — avoids double-escaping.
    },
  });
