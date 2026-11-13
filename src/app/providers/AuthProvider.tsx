import { useEffect, useRef, useState, type ReactNode } from 'react';
import { useTranslation } from 'react-i18next';
import { configureApiClientAuth } from '@/shared/lib/apiClient';
import { decodeJwtPayload } from '@/shared/utils/jwt';
import { useAuthStore } from '@/features/auth/store/authStore';
import { refreshSession } from '@/features/auth/api/authApi';
import { getPatientProfile, updateMyProfile } from '@/features/patient-dashboard/api/patientApi';
import { getDoctorProfile, updateMyDoctorProfile } from '@/features/doctor-dashboard/api/doctorApi';
import type { AuthenticatedUser } from '@/shared/types/api';

// Wired once at module load — see the layering note in shared/lib/apiClient.ts
// for why this indirection exists instead of apiClient importing authStore directly.
configureApiClientAuth({
  getAccessToken: () => useAuthStore.getState().accessToken,
  onTokenRefreshed: (newAccessToken) => useAuthStore.getState().setAccessToken(newAccessToken),
  onAuthFailure: () => useAuthStore.getState().clearSession(),
});

/** Turns a decoded token + fetched profile into the session's AuthenticatedUser. */
function buildAuthenticatedUser(
  role: 'Patient' | 'Doctor',
  userId: string,
  profile: {
    id: string;
    fullName: string;
    email: string;
    preferredLanguage: 'ar' | 'en';
    specialty?: string;
  },
): AuthenticatedUser {
  return {
    id: userId,
    role,
    fullName: profile.fullName,
    email: profile.email,
    preferredLanguage: profile.preferredLanguage,
    patientId: role === 'Patient' ? profile.id : undefined,
    doctorId: role === 'Doctor' ? profile.id : undefined,
    specialty: role === 'Doctor' ? profile.specialty : undefined,
  };
}

interface AuthProviderProps {
  children: ReactNode;
}

export function AuthProvider({ children }: AuthProviderProps) {
  const { i18n } = useTranslation();
  const status = useAuthStore((state) => state.status);
  const setSession = useAuthStore((state) => state.setSession);
  const clearSession = useAuthStore((state) => state.clearSession);
  const [bootstrapped, setBootstrapped] = useState(false);
  const hasBootstrapped = useRef(false);

  // 1. On first mount, try to silently resume a session using the httpOnly
  //    refresh cookie — this is what makes a hard page refresh not bounce an
  //    already-logged-in user back to /auth.
  useEffect(() => {
    if (hasBootstrapped.current) return;
    hasBootstrapped.current = true;

    async function bootstrap() {
      try {
        const { accessToken } = await refreshSession();
        const decoded = decodeJwtPayload(accessToken);
        if (!decoded) throw new Error('Malformed access token');

        useAuthStore.getState().setAccessToken(accessToken);
        const profile =
          decoded.role === 'Doctor' ? await getDoctorProfile() : await getPatientProfile();

        setSession(accessToken, buildAuthenticatedUser(decoded.role, decoded.sub, profile));
        void i18n.changeLanguage(profile.preferredLanguage);
      } catch {
        // No valid refresh cookie (or it expired) — that's a normal, expected
        // outcome for a first-time or logged-out visitor, not an error to surface.
        clearSession();
      } finally {
        setBootstrapped(true);
      }
    }

    void bootstrap();
  }, [clearSession, i18n, setSession]);

  // 2. Persist language changes to the authenticated user's backend profile
  //    (frontend-architecture.md §9: "persisted to the user's profile ... so
  //    it follows the user across devices rather than resetting per browser").
  //    Fire-and-forget: a failed save shouldn't block switching the UI language.
  useEffect(() => {
    const handleLanguageChanged = (lng: string) => {
      const currentUser = useAuthStore.getState().user;
      if (!currentUser || (lng !== 'ar' && lng !== 'en')) return;
      if (currentUser.preferredLanguage === lng) return;

      const save =
        currentUser.role === 'Doctor'
          ? updateMyDoctorProfile({ preferredLanguage: lng })
          : updateMyProfile({ preferredLanguage: lng });

      save.catch(() => console.error('Failed to persist language preference'));
    };

    i18n.on('languageChanged', handleLanguageChanged);
    return () => i18n.off('languageChanged', handleLanguageChanged);
  }, [i18n]);

  // Avoid flashing the login page for an already-authenticated user while the
  // silent-refresh bootstrap above is still in flight.
  if (!bootstrapped && status === 'idle') {
    return (
      <div className="flex min-h-screen items-center justify-center bg-bg text-text-muted">
        <span className="font-mono text-sm">CardioSense</span>
      </div>
    );
  }

  return <>{children}</>;
}
