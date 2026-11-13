import type { ReactElement } from 'react';
import { describe, expect, it } from 'vitest';
import { render, screen } from '@testing-library/react';
import i18next from 'i18next';
import { I18nextProvider, initReactI18next } from 'react-i18next';
import en from '@/locales/en/translation.json';
import ar from '@/locales/ar/translation.json';
import { StatusPill } from '@/shared/components/StatusPill';

// A separate, real i18next instance for tests — deliberately not mocked, so
// this test also doubles as a check that the actual translation keys exist.
const testI18n = i18next.createInstance();
void testI18n.use(initReactI18next).init({
  resources: { en: { translation: en }, ar: { translation: ar } },
  lng: 'en',
  interpolation: { escapeValue: false },
});

function renderWithI18n(ui: ReactElement) {
  return render(<I18nextProvider i18n={testI18n}>{ui}</I18nextProvider>);
}

describe('StatusPill', () => {
  it('renders the default translated label for Stable', () => {
    renderWithI18n(<StatusPill status="Stable" />);
    expect(screen.getByText('Stable')).toBeInTheDocument();
  });

  it('renders the default translated label for Critical', () => {
    renderWithI18n(<StatusPill status="Critical" />);
    expect(screen.getByText('Critical')).toBeInTheDocument();
  });

  it('prefers a custom label override over the default translation', () => {
    renderWithI18n(<StatusPill status="Stable" label="Fully Stable" />);
    expect(screen.getByText('Fully Stable')).toBeInTheDocument();
    expect(screen.queryByText('Stable')).not.toBeInTheDocument();
  });
});
