import { lazy, Suspense, type ReactElement } from 'react';
import { Navigate, Route, Routes } from 'react-router-dom';
import { useTranslation } from 'react-i18next';
import { ProtectedRoute } from '@/routes/ProtectedRoute';
import { PatientLayout } from '@/app/layouts/PatientLayout';
import { DoctorLayout } from '@/app/layouts/DoctorLayout';
import { AuthPage } from '@/features/auth/pages/AuthPage';
import { useAuthStore } from '@/features/auth/store/authStore';

// Route-level code splitting: the sign-in page (the first screen every visitor sees) loads on its own, and the
// dashboards, which pull in the charting library and the live-ECG code, are downloaded only after sign-in.
// The pages use named exports, hence the small `.then()` adapter that React.lazy requires.
const PatientDashboardPage = lazy(() =>
  import('@/features/patient-dashboard/pages/PatientDashboardPage').then((module) => ({
    default: module.PatientDashboardPage,
  })),
);
const PatientListPage = lazy(() =>
  import('@/features/doctor-dashboard/pages/PatientListPage').then((module) => ({
    default: module.PatientListPage,
  })),
);
const PatientDetailPage = lazy(() =>
  import('@/features/doctor-dashboard/pages/PatientDetailPage').then((module) => ({
    default: module.PatientDetailPage,
  })),
);
const SettingsPage = lazy(() =>
  import('@/features/settings/pages/SettingsPage').then((module) => ({
    default: module.SettingsPage,
  })),
);

/** Shown inside the layout (so the sidebar / top bar stay put) while a page chunk is downloading. */
function RouteFallback() {
  const { t } = useTranslation();
  return (
    <div role="status" className="flex min-h-[40vh] items-center justify-center text-sm text-text-muted">
      {t('common.loading')}
    </div>
  );
}

/** Wraps a lazily-loaded page in its own Suspense boundary. */
function withSuspense(page: ReactElement): ReactElement {
  return <Suspense fallback={<RouteFallback />}>{page}</Suspense>;
}

/** "/" has no page of its own — it just forwards to the right home route. */
function RootRedirect() {
  const user = useAuthStore((state) => state.user);
  if (!user) return <Navigate to="/auth" replace />;
  return <Navigate to={user.role === 'Doctor' ? '/doctor/patients' : '/patient'} replace />;
}

/**
 * `/settings` (frontend-architecture.md §5: "any authenticated role") needs
 * *a* layout's chrome around it, but which one depends on who's logged in —
 * this picks the matching shell so a doctor still sees their sidebar and a
 * patient still sees their minimal top bar on the very same route.
 */
function RoleAwareLayout() {
  const user = useAuthStore((state) => state.user);
  return user?.role === 'Doctor' ? <DoctorLayout /> : <PatientLayout />;
}

export function AppRouter() {
  return (
    <Routes>
      <Route path="/" element={<RootRedirect />} />
      <Route path="/auth" element={<AuthPage />} />

      {/* Patient-only area */}
      <Route element={<ProtectedRoute allow={['Patient']} />}>
        <Route element={<PatientLayout />}>
          <Route path="/patient" element={withSuspense(<PatientDashboardPage />)} />
        </Route>
      </Route>

      {/* Doctor-only area */}
      <Route element={<ProtectedRoute allow={['Doctor']} />}>
        <Route element={<DoctorLayout />}>
          <Route path="/doctor/patients" element={withSuspense(<PatientListPage />)} />
          <Route path="/doctor/patients/:patientId" element={withSuspense(<PatientDetailPage />)} />
        </Route>
      </Route>

      {/* Shared area — any authenticated role */}
      <Route element={<ProtectedRoute allow={['Patient', 'Doctor']} />}>
        <Route element={<RoleAwareLayout />}>
          <Route path="/settings" element={withSuspense(<SettingsPage />)} />
        </Route>
      </Route>

      <Route path="*" element={<RootRedirect />} />
    </Routes>
  );
}
