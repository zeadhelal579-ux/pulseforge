/**
 * File: src/shared/components/DemoBanner.tsx
 * Layer: shared/components
 * Purpose: A slim strip shown at the top of every layout while the app runs on the in-browser demo backend,
 *          so nobody mistakes simulated data for a live clinical system or types real personal data into it.
 * Logic: Renders nothing outside demo mode. The "Reset demo data" action wipes the browser-only database and
 *        session, then reloads the app at the sign-in page so it reseeds from scratch.
 * Implementation: pure presentational component; hooks run before the early return to respect the rules of hooks.
 */
import { useTranslation } from 'react-i18next';
import { resetDemoData } from '@/shared/demo/demoStorage';
import { IS_DEMO_MODE } from '@/shared/lib/env';

export function DemoBanner() {
  const { t } = useTranslation();
  if (!IS_DEMO_MODE) return null;

  /** Clears the demo database + session and returns to the sign-in page. */
  const handleReset = () => {
    resetDemoData();
    window.location.assign('/auth');
  };

  return (
    <div
      role="status"
      className="flex flex-wrap items-center justify-center gap-x-3 gap-y-1 border-b border-warning/30 bg-warning/10 px-4 py-1.5 text-center text-xs text-warning"
    >
      <span>{t('demo.banner')}</span>
      <button
        type="button"
        onClick={handleReset}
        className="font-bold underline underline-offset-2 hover:no-underline"
      >
        {t('demo.reset')}
      </button>
    </div>
  );
}
