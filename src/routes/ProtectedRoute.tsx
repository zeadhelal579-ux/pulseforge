import { Navigate, Outlet } from 'react-router-dom';
import { useAuthStore } from '@/features/auth/store/authStore';
import type { UserRole } from '@/shared/types/api';

interface ProtectedRouteProps {
  allow: UserRole[];
}

/**
 * Wraps a set of nested <Route> elements. Redirects to /auth if there's no
 * session at all, or to the correct home route if the session exists but
 * doesn't have one of the allowed roles (e.g. a patient hitting a /doctor/*
 * URL directly) — a client-side redirect only prevents an honest user from
 * landing on the wrong screen; it does nothing against someone editing
 * requests directly, which is why the backend check is the real boundary.
 */
export function ProtectedRoute({ allow }: ProtectedRouteProps) {
  const user = useAuthStore((state) => state.user);

  if (!user) return <Navigate to="/auth" replace />;

  if (!allow.includes(user.role)) {
    return <Navigate to={user.role === 'Doctor' ? '/doctor/patients' : '/patient'} replace />;
  }

  return <Outlet />;
}
