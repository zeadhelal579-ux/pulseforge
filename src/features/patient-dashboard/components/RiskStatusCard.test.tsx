import type { ReactElement } from 'react';
import { describe, expect, it } from 'vitest';
import { render, screen } from '@testing-library/react';
import i18next from 'i18next';
import { I18nextProvider, initReactI18next } from 'react-i18next';
import en from '@/locales/en/translation.json';
import ar from '@/locales/ar/translation.json';
import { RiskStatusCard } from '@/features/patient-dashboard/components/RiskStatusCard';

// A separate, real i18next instance for tests — deliberately not mocked, so
// this test also doubles as a check that the actual translation keys exist
// (same pattern as StatusPill.test.tsx).
const testI18n = i18next.createInstance();
void testI18n.use(initReactI18next).init({
  resources: { en: { translation: en }, ar: { translation: ar } },
  lng: 'en',
  interpolation: { escapeValue: false },
});

function renderWithI18n(ui: ReactElement) {
  return render(<I18nextProvider i18n={testI18n}>{ui}</I18nextProvider>);
}

/**
 * This is the single most safety-critical UI rule in the whole app
 * (backend-database-architecture.md §6, PROJECT_OVERVIEW.md §4): a patient
 * must never see the raw numeric risk score, only a Stable/Caution/Critical
 * status — a bare number either falsely reassures or panics someone with no
 * clinical training to interpret it. Before this file, that rule had zero
 * automated coverage; every other test in this suite is a pure utility
 * function, so a future change to RiskStatusCard could silently start
 * leaking the number to patients and nothing would fail.
 */
describe('RiskStatusCard — showNumericScore gating', () => {
  it('never renders the numeric score when showNumericScore is false, even if riskScore is provided', () => {
    renderWithI18n(<RiskStatusCard statusLevel="Caution" riskScore={42} showNumericScore={false} />);
    expect(screen.queryByText('42')).not.toBeInTheDocument();
    expect(screen.queryByText('/100')).not.toBeInTheDocument();
    // The patient-facing fallback (translated status text) must still
    // appear — it legitimately renders twice here (once in the StatusPill
    // badge, once in the prose fallback below it), hence getAllByText
    // rather than getByText.
    expect(screen.getAllByText('Caution').length).toBeGreaterThan(0);
  });

  it('renders the numeric score when showNumericScore is true and riskScore is provided', () => {
    renderWithI18n(<RiskStatusCard statusLevel="Critical" riskScore={91} showNumericScore={true} />);
    expect(screen.getByText('91')).toBeInTheDocument();
    expect(screen.getByText('/100')).toBeInTheDocument();
  });

  it('falls back to the status text (no crash) when showNumericScore is true but riskScore is missing', () => {
    renderWithI18n(<RiskStatusCard statusLevel="Stable" riskScore={undefined} showNumericScore={true} />);
    expect(screen.queryByText('/100')).not.toBeInTheDocument();
    expect(screen.getAllByText('Stable').length).toBeGreaterThan(0);
  });

  it('shows the empty-state text (no crash) when there is no status yet', () => {
    renderWithI18n(<RiskStatusCard statusLevel={null} riskScore={77} showNumericScore={false} />);
    expect(screen.queryByText('77')).not.toBeInTheDocument();
    expect(screen.getByText('No data yet.')).toBeInTheDocument();
  });
});
