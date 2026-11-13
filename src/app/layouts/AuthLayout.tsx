import type { ReactNode } from 'react';
import { useTranslation } from 'react-i18next';
import { ShieldCheck, Activity, BrainCircuit } from 'lucide-react';
import { LanguageToggle } from '@/shared/components/LanguageToggle';
import { ThemeToggle } from '@/shared/components/ThemeToggle';
import { Badge } from '@/shared/components/ui/Badge';
import { DemoBanner } from '@/shared/components/DemoBanner';

interface AuthLayoutProps {
  /** The tab bar + role toggle + active form — supplied by AuthPage. */
  children: ReactNode;
}

/**
 * Decorative, looping ECG trace — purely visual (see the `.ecg-hero-path`
 * keyframes in styles/globals.css). Not the same component as the real
 * live-data canvas in the dashboards.
 */
function HeroEcgTrace() {
  return (
    <svg viewBox="0 0 400 100" className="h-20 w-full" aria-hidden="true">
      <polyline
        className="ecg-hero-path"
        points="0,50 60,50 75,20 90,80 105,50 160,50 175,35 185,65 195,50 240,50 255,15 270,85 285,50 400,50"
        fill="none"
        stroke="var(--color-accent)"
        strokeWidth="2.5"
        strokeLinecap="round"
        strokeLinejoin="round"
      />
    </svg>
  );
}

export function AuthLayout({ children }: AuthLayoutProps) {
  const { t } = useTranslation();

  const features = [
    { icon: Activity, text: t('auth.feature1') },
    { icon: ShieldCheck, text: t('auth.feature2') },
    { icon: BrainCircuit, text: t('auth.feature3') },
  ];

  return (
    <div className="min-h-screen bg-bg">
      <DemoBanner />
      <header className="flex items-center justify-between border-b border-border px-6 py-4 md:px-10">
        <div>
          <p className="text-lg font-extrabold text-text-primary">{t('common.appName')}</p>
          <p className="font-mono text-[11px] tracking-wide text-text-muted">
            {t('auth.brandSub')}
          </p>
        </div>
        <div className="flex items-center gap-3">
          <ThemeToggle />
          <LanguageToggle />
        </div>
      </header>

      <main className="mx-auto grid max-w-6xl gap-10 px-6 py-10 md:grid-cols-2 md:gap-16 md:px-10 md:py-16">
        {/* Hero column */}
        <section className="flex flex-col justify-center">
          <Badge status="info" className="mb-5 w-fit">
            {t('auth.badge')}
          </Badge>
          <h1 className="mb-4 text-3xl font-extrabold leading-tight text-text-primary md:text-4xl">
            {t('auth.heroHeadline')}
          </h1>
          <p className="mb-8 max-w-md text-[15px] leading-relaxed text-text-secondary">
            {t('auth.heroSub')}
          </p>

          <ul className="mb-8 flex flex-col gap-4">
            {features.map(({ icon: Icon, text }) => (
              <li key={text} className="flex items-center gap-3 text-sm text-text-secondary">
                <span className="flex size-8 shrink-0 items-center justify-center rounded-lg bg-accent-subtle text-accent">
                  <Icon className="size-4" />
                </span>
                {text}
              </li>
            ))}
          </ul>

          <div className="max-w-md rounded-xl border border-border bg-surface p-4">
            <HeroEcgTrace />
            <p className="mt-2 text-xs text-text-muted">{t('auth.heroFooter')}</p>
          </div>
        </section>

        {/* Form card column */}
        <section className="flex items-start justify-center md:justify-end">
          <div className="w-full max-w-md rounded-2xl border border-border bg-surface p-6 shadow-xl md:p-8">
            {children}
          </div>
        </section>
      </main>
    </div>
  );
}
