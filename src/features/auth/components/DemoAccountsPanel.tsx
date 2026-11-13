/**
 * File: src/features/auth/components/DemoAccountsPanel.tsx
 * Layer: features/auth
 * Purpose: The "Demo accounts (click to fill)" list under the sign-in form: one click fills the email and
 *          password fields and signs in, so the demo can be entered in a second as doctor or as a patient.
 * Logic: The panel only reports which account was picked (`onPick`); LoginForm owns the form state and the
 *        login mutation, so there is exactly one sign-in code path. Rows are disabled while a sign-in is in
 *        flight and the clicked row shows a spinner.
 * Implementation: a list of buttons styled with the shared design tokens; account data comes from
 *        shared/demo/demoAccounts.ts (single source of truth, also used to seed the demo database).
 */
import { Loader2, Stethoscope, UserRound } from 'lucide-react';
import { useTranslation } from 'react-i18next';
import { StatusPill } from '@/shared/components/StatusPill';
import { DEMO_ACCOUNTS, type DemoAccount, type DemoAccountId } from '@/shared/demo/demoAccounts';
import { DEMO_PASSWORD } from '@/shared/demo/demoConstants';
import { resetDemoData } from '@/shared/demo/demoStorage';
import { cn } from '@/shared/utils/cn';

interface DemoAccountsPanelProps {
  /** Account whose sign-in is currently running, if any. */
  pendingId: DemoAccountId | null;
  /** Disables every row (a sign-in is already in flight). */
  disabled: boolean;
  /** Called with the picked account; the parent fills the form and submits. */
  onPick: (account: DemoAccount) => void;
}

export function DemoAccountsPanel({ pendingId, disabled, onPick }: DemoAccountsPanelProps) {
  const { t } = useTranslation();

  /** Wipes the browser-only database and session, then reloads so the demo reseeds. */
  const handleReset = () => {
    resetDemoData();
    window.location.reload();
  };

  return (
    <section className="mt-6 border-t border-dashed border-border pt-5" aria-labelledby="demo-accounts-title">
      <h2 id="demo-accounts-title" className="mb-1 text-[13px] font-semibold text-text-secondary">
        {t('demo.accountsTitle')}
      </h2>
      <p className="mb-3 text-xs text-text-muted">
        {t('demo.passwordHint')}{' '}
        <bdi dir="ltr" className="font-mono text-text-secondary">
          {DEMO_PASSWORD}
        </bdi>
      </p>

      <ul className="flex flex-col gap-2">
        {DEMO_ACCOUNTS.map((account) => (
          <li key={account.id}>
            <button
              type="button"
              onClick={() => onPick(account)}
              disabled={disabled}
              className={cn(
                'flex w-full items-center gap-3 rounded-[10px] border border-border bg-surface-alt px-3 py-2.5 text-start transition-colors',
                'hover:border-accent hover:bg-accent-subtle',
                'focus-visible:outline focus-visible:outline-2 focus-visible:outline-offset-2 focus-visible:outline-accent',
                'disabled:cursor-not-allowed disabled:opacity-60',
              )}
            >
              <span className="flex size-8 shrink-0 items-center justify-center rounded-lg bg-accent-subtle text-accent">
                {pendingId === account.id ? (
                  <Loader2 className="size-4 animate-spin" aria-hidden="true" />
                ) : account.role === 'Doctor' ? (
                  <Stethoscope className="size-4" aria-hidden="true" />
                ) : (
                  <UserRound className="size-4" aria-hidden="true" />
                )}
              </span>
              <span className="min-w-0 flex-1">
                <span className="flex flex-wrap items-center gap-2 text-[13px] font-semibold text-text-primary">
                  {account.role === 'Doctor'
                    ? t('demo.accounts.doctor')
                    : t('demo.accounts.patient')}
                  {account.status && <StatusPill status={account.status} />}
                </span>
                <span className="block truncate font-mono text-[11.5px] text-text-muted">
                  <bdi dir="ltr">{account.email}</bdi>
                </span>
              </span>
            </button>
          </li>
        ))}
      </ul>

      <p className="mt-3 text-xs text-text-muted">
        {t('demo.storageNote')}{' '}
        <button type="button" onClick={handleReset} className="font-bold text-accent hover:underline">
          {t('demo.reset')}
        </button>
      </p>
    </section>
  );
}
