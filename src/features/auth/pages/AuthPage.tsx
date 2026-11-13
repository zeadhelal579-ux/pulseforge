import { useState } from 'react';
import { Navigate } from 'react-router-dom';
import { useTranslation } from 'react-i18next';
import { AuthLayout } from '@/app/layouts/AuthLayout';
import { Tabs } from '@/shared/components/ui/Tabs';
import { SignupForm } from '@/features/auth/components/SignupForm';
import { LoginForm } from '@/features/auth/components/LoginForm';
import { useAuthStore } from '@/features/auth/store/authStore';
import type { AuthTab } from '@/features/auth/types';

export function AuthPage() {
  const { t } = useTranslation();
  const user = useAuthStore((state) => state.user);
  const [tab, setTab] = useState<AuthTab>('signup');

  // An already-authenticated user landing on /auth (e.g. via browser back)
  // should go straight to their dashboard, not see the form again.
  if (user) {
    return <Navigate to={user.role === 'Doctor' ? '/doctor/patients' : '/patient'} replace />;
  }

  return (
    <AuthLayout>
      <Tabs<AuthTab>
        value={tab}
        onChange={setTab}
        className="mb-6"
        tabs={[
          { value: 'signup', label: t('auth.tabSignup') },
          { value: 'login', label: t('auth.tabLogin') },
        ]}
      />

      {tab === 'signup' ? <SignupForm /> : <LoginForm />}

      <p className="mt-5 text-center text-[13px] text-text-secondary">
        {tab === 'signup' ? (
          <>
            {t('auth.switchToLogin')}
            <button
              type="button"
              onClick={() => setTab('login')}
              className="font-bold text-accent hover:underline"
            >
              {t('auth.switchToLoginLink')}
            </button>
          </>
        ) : (
          <>
            {t('auth.switchToSignup')}
            <button
              type="button"
              onClick={() => setTab('signup')}
              className="font-bold text-accent hover:underline"
            >
              {t('auth.switchToSignupLink')}
            </button>
          </>
        )}
      </p>
    </AuthLayout>
  );
}
