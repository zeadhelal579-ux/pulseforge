import { NavLink, Outlet, useNavigate } from 'react-router-dom';
import { useTranslation } from 'react-i18next';
import { Bell, ClipboardList, LayoutGrid, LogOut, Radio, Settings, Users } from 'lucide-react';
import { LanguageToggle } from '@/shared/components/LanguageToggle';
import { ThemeToggle } from '@/shared/components/ThemeToggle';
import { DemoBanner } from '@/shared/components/DemoBanner';
import { useSignalR, type SignalRConnectionState } from '@/shared/hooks/useSignalR';
import { cn } from '@/shared/utils/cn';
import { useAuthStore } from '@/features/auth/store/authStore';
import { logout } from '@/features/auth/api/authApi';
import { useWardStats } from '@/features/doctor-dashboard/hooks/useWardStats';

const NAV_LINK_BASE =
  'flex items-center gap-3 rounded-lg px-3 py-2.5 text-sm font-medium transition-colors';

/**
 * Label and colours for each real-time connection state. The sidebar used to show a hard-coded "Stable"
 * badge, which kept reading "Stable" even with the socket down: a monitoring screen must never reassure
 * by default, so it now reflects the actual SignalR connection.
 */
const CONNECTION_DISPLAY: Record<
  SignalRConnectionState,
  { labelKey: string; textClass: string; dotClass: string }
> = {
  connected: {
    labelKey: 'nav.connection.connected',
    textClass: 'text-accent',
    dotClass: 'bg-accent',
  },
  connecting: {
    labelKey: 'nav.connection.connecting',
    textClass: 'text-warning',
    dotClass: 'bg-warning',
  },
  reconnecting: {
    labelKey: 'nav.connection.reconnecting',
    textClass: 'text-warning',
    dotClass: 'bg-warning',
  },
  disconnected: {
    labelKey: 'nav.connection.disconnected',
    textClass: 'text-danger',
    dotClass: 'bg-danger',
  },
};

export function DoctorLayout() {
  const { t } = useTranslation();
  const navigate = useNavigate();
  const user = useAuthStore((state) => state.user);
  const clearSession = useAuthStore((state) => state.clearSession);
  const { connectionState } = useSignalR();
  const connection = CONNECTION_DISPLAY[connectionState];

  // Powers the "12 Active" badge in the sidebar — a lightweight slice of the
  // same ward-stats endpoint the patient-list page renders in full.
  const { data: wardStats } = useWardStats();

  const handleLogout = async () => {
    try {
      await logout();
    } finally {
      clearSession();
      navigate('/auth', { replace: true });
    }
  };

  return (
    <div className="flex min-h-screen flex-col">
      <DemoBanner />
      <div className="grid flex-1 grid-cols-1 md:grid-cols-[260px_1fr]">
        {/* Sidebar */}
        <aside className="flex flex-col border-e border-border bg-surface p-4">
          <div className="mb-6 px-2">
            <p className="text-lg font-extrabold text-text-primary">{t('common.appName')}</p>
            <p className="font-mono text-[10px] tracking-wide text-text-muted">
              {t('auth.brandSub')}
            </p>
          </div>

          <div className="mb-5 flex items-center justify-between rounded-lg bg-surface-alt px-3 py-2 text-xs">
            <span className="text-text-secondary">{t('nav.connectionStatus')}</span>
            <span className={cn('flex items-center gap-1.5 font-semibold', connection.textClass)}>
              <span className={cn('size-1.5 rounded-full', connection.dotClass)} />
              {t(connection.labelKey)}
            </span>
          </div>

          <nav className="flex flex-1 flex-col gap-1">
            <NavLink
              to="/doctor/patients"
              className={({ isActive }) =>
                cn(
                  NAV_LINK_BASE,
                  isActive
                    ? 'bg-accent-subtle text-accent'
                    : 'text-text-secondary hover:bg-surface-alt hover:text-text-primary',
                )
              }
            >
              <LayoutGrid className="size-4" />
              {t('nav.liveMonitoring')}
              {typeof wardStats?.activePatients === 'number' && (
                <span className="ms-auto rounded-md bg-accent-subtle px-1.5 py-0.5 font-mono text-[11px] text-accent">
                  {wardStats.activePatients} {t('nav.active')}
                </span>
              )}
            </NavLink>
            <NavLink
              to="/doctor/patients"
              className={({ isActive }) =>
                cn(
                  NAV_LINK_BASE,
                  isActive
                    ? 'bg-accent-subtle text-accent'
                    : 'text-text-secondary hover:bg-surface-alt hover:text-text-primary',
                )
              }
            >
              <Users className="size-4" />
              {t('nav.patients')}
            </NavLink>
            <button
              type="button"
              className={cn(NAV_LINK_BASE, 'text-text-muted opacity-60')}
              disabled
              title={t('nav.comingSoon')}
            >
              <ClipboardList className="size-4" />
              {t('nav.reports')}
            </button>
            <NavLink
              to="/settings"
              className={({ isActive }) =>
                cn(
                  NAV_LINK_BASE,
                  isActive
                    ? 'bg-accent-subtle text-accent'
                    : 'text-text-secondary hover:bg-surface-alt hover:text-text-primary',
                )
              }
            >
              <Settings className="size-4" />
              {t('nav.settings')}
            </NavLink>
          </nav>

          <button
            type="button"
            onClick={handleLogout}
            className={cn(NAV_LINK_BASE, 'text-text-muted hover:bg-danger/10 hover:text-danger')}
          >
            <LogOut className="size-4" />
            {t('common.logout')}
          </button>

          <p className="mt-4 px-2 font-mono text-[10px] text-text-muted">{t('nav.prototypeBuild')}</p>
        </aside>

        {/* Main column */}
        <div className="flex flex-col">
          <header className="flex flex-wrap items-center justify-between gap-3 border-b border-border bg-surface px-4 py-3 md:px-6">
            <div>
              <h1 className="text-base font-bold text-text-primary md:text-lg">
                {t('nav.clinicalSystemTitle')}
              </h1>
              <p className="text-xs text-text-muted">{t('nav.cardiacCareUnit')}</p>
            </div>
            <div className="flex items-center gap-3">
              <ThemeToggle />
              <LanguageToggle />
              <button
                type="button"
                aria-label={t('nav.notifications')}
                title={t('nav.comingSoon')}
                disabled
                className="flex size-9 items-center justify-center rounded-full border border-border bg-surface-alt text-text-secondary opacity-60"
              >
                <Bell className="size-4" />
              </button>
              <span className="hidden items-center gap-1.5 text-xs text-accent md:flex">
                <Radio className="size-3.5" />
              </span>
              <div className="flex items-center gap-2 border-s border-border ps-3">
                <div className="flex size-8 items-center justify-center rounded-full bg-accent-subtle text-xs font-bold text-accent">
                  {user?.fullName?.charAt(0) ?? 'D'}
                </div>
                <div className="hidden text-start sm:block">
                  <p className="text-sm font-semibold leading-tight text-text-primary">
                    {user?.fullName}
                  </p>
                  <p className="text-xs leading-tight text-text-muted">{user?.specialty}</p>
                </div>
              </div>
            </div>
          </header>

          <main className="flex-1 overflow-y-auto p-4 md:p-6">
            <Outlet />
          </main>
        </div>
      </div>
    </div>
  );
}
