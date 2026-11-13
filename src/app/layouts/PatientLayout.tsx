import { Link, Outlet, useNavigate } from 'react-router-dom';
import { useTranslation } from 'react-i18next';
import { LogOut, Settings } from 'lucide-react';
import { LanguageToggle } from '@/shared/components/LanguageToggle';
import { ThemeToggle } from '@/shared/components/ThemeToggle';
import { useAuthStore } from '@/features/auth/store/authStore';
import { logout } from '@/features/auth/api/authApi';

export function PatientLayout() {
  const { t } = useTranslation();
  const navigate = useNavigate();
  const clearSession = useAuthStore((state) => state.clearSession);

  const handleLogout = async () => {
    try {
      await logout();
    } finally {
      clearSession();
      navigate('/auth', { replace: true });
    }
  };

  return (
    <div className="min-h-screen bg-bg">
      <header className="flex items-center justify-between border-b border-border bg-surface px-4 py-3 md:px-6">
        <div>
          <p className="text-base font-extrabold text-text-primary">{t('common.appName')}</p>
          <p className="font-mono text-[10px] tracking-wide text-text-muted">
            {t('auth.brandSub')}
          </p>
        </div>
        <div className="flex items-center gap-2">
          <ThemeToggle />
          <LanguageToggle />
          <Link
            to="/settings"
            aria-label={t('nav.settings')}
            className="flex size-9 items-center justify-center rounded-full border border-border bg-surface-alt text-text-secondary hover:text-accent"
          >
            <Settings className="size-4" />
          </Link>
          <button
            type="button"
            onClick={handleLogout}
            aria-label={t('common.logout')}
            className="flex size-9 items-center justify-center rounded-full border border-border bg-surface-alt text-text-secondary hover:text-danger"
          >
            <LogOut className="size-4" />
          </button>
        </div>
      </header>

      <main className="mx-auto max-w-5xl p-4 md:p-6">
        <Outlet />
      </main>
    </div>
  );
}
