import { Navigate, Route, Routes } from 'react-router-dom';
import { ProtectedRoute } from '@/routes/ProtectedRoute';
import { PatientLayout } from '@/app/layouts/PatientLayout';
import { DoctorLayout } from '@/app/layouts/DoctorLayout';
import { AuthPage } from '@/features/auth/pages/AuthPage';
import { PatientDashboardPage } from '@/features/patient-dashboard/pages/PatientDashboardPage';
import { PatientListPage } from '@/features/doctor-dashboard/pages/PatientListPage';
import { PatientDetailPage } from '@/features/doctor-dashboard/pages/PatientDetailPage';
import { SettingsPage } from '@/features/settings/pages/SettingsPage';
import { useAuthStore } from '@/features/auth/store/authStore';

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
          <Route path="/patient" element={<PatientDashboardPage />} />
        </Route>
      </Route>

      {/* Doctor-only area */}
      <Route element={<ProtectedRoute allow={['Doctor']} />}>
        <Route element={<DoctorLayout />}>
          <Route path="/doctor/patients" element={<PatientListPage />} />
          <Route path="/doctor/patients/:patientId" element={<PatientDetailPage />} />
        </Route>
      </Route>

      {/* Shared area — any authenticated role */}
      <Route element={<ProtectedRoute allow={['Patient', 'Doctor']} />}>
        <Route element={<RoleAwareLayout />}>
          <Route path="/settings" element={<SettingsPage />} />
        </Route>
      </Route>

      <Route path="*" element={<RootRedirect />} />
    </Routes>
  );
}
