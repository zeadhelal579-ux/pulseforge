import { useState } from 'react';
import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { useMutation } from '@tanstack/react-query';
import { useTranslation } from 'react-i18next';
import { useNavigate } from 'react-router-dom';
import { Input } from '@/shared/components/ui/Input';
import { PasswordInput } from '@/shared/components/ui/PasswordInput';
import { Button } from '@/shared/components/ui/Button';
import { login } from '@/features/auth/api/authApi';
import { DemoAccountsPanel } from '@/features/auth/components/DemoAccountsPanel';
import { useAuthStore } from '@/features/auth/store/authStore';
import { loginSchema, type LoginFormValues } from '@/features/auth/schemas/authSchemas';
import type { DemoAccount, DemoAccountId } from '@/shared/demo/demoAccounts';
import { IS_DEMO_MODE } from '@/shared/lib/env';

export function LoginForm() {
  const { t, i18n } = useTranslation();
  const navigate = useNavigate();
  const setSession = useAuthStore((state) => state.setSession);

  // Which demo account (if any) is being signed in, so its row can show a spinner.
  const [pendingDemoId, setPendingDemoId] = useState<DemoAccountId | null>(null);

  const {
    register,
    handleSubmit,
    setValue,
    formState: { errors },
  } = useForm<LoginFormValues>({ resolver: zodResolver(loginSchema) });

  const mutation = useMutation({
    mutationFn: login,
    onSuccess: (response) => {
      setSession(response.accessToken, response.user);
      void i18n.changeLanguage(response.user.preferredLanguage);
      navigate(response.user.role === 'Doctor' ? '/doctor/patients' : '/patient', {
        replace: true,
      });
    },
    onSettled: () => setPendingDemoId(null),
  });

  const onSubmit = handleSubmit((values) => mutation.mutate(values));

  /** Demo shortcut: shows the account's credentials in the fields, then signs in with them straight away. */
  const handleDemoPick = (account: DemoAccount) => {
    setPendingDemoId(account.id);
    setValue('email', account.email, { shouldValidate: true });
    setValue('password', account.password, { shouldValidate: true });
    mutation.mutate({ email: account.email, password: account.password });
  };

  return (
    <>
      <form onSubmit={onSubmit} noValidate>
        <Input
          label={t('auth.fields.email')}
          type="email"
          placeholder={t('auth.fields.emailPh')}
          required
          error={errors.email && t(errors.email.message ?? 'auth.errors.required')}
          {...register('email')}
        />

        <PasswordInput
          label={t('auth.fields.password')}
          placeholder={t('auth.fields.passwordPh')}
          required
          error={errors.password && t(errors.password.message ?? 'auth.errors.required')}
          {...register('password')}
        />

        <div className="mb-4 text-end">
          <Button type="button" variant="link" size="sm" className="!p-0 !text-xs font-semibold">
            {t('auth.forgotPassword')}
          </Button>
        </div>

        {mutation.isError && (
          <p className="mb-4 rounded-lg bg-danger/10 px-3 py-2 text-xs text-danger">
            {t('auth.errors.loginFailed')}
          </p>
        )}

        <Button type="submit" size="lg" isLoading={mutation.isPending} className="mb-2">
          {t('auth.submitLogin')}
        </Button>
      </form>

      {IS_DEMO_MODE && (
        <DemoAccountsPanel
          pendingId={pendingDemoId}
          disabled={mutation.isPending}
          onPick={handleDemoPick}
        />
      )}
    </>
  );
}
