# Source file map

The tree and inventory are generated from the maintained local source roots. Each inventory row identifies the file responsibility and the owning entry point or consumer. Generated migrations, lockfiles and build output have separate authorities. The Vercel snapshot remains separate.

## Web

Root: `front-end/CardioSense-Frontend-fixed`. 110 listed files and 110 inventory entries.

```text
.env.example
.eslintrc.cjs
.gitignore
.prettierrc
HANDOFF_REPORT.md
index.html
package.json
postcss.config.js
public/
  brand/
    pulseforge-logo.jpeg
  favicon.svg
README.md
src/
  app/
    App.tsx
    layouts/
      AuthLayout.tsx
      DoctorLayout.tsx
      PatientLayout.tsx
    providers/
      AuthProvider.tsx
      i18n.ts
      I18nProvider.tsx
      QueryProvider.tsx
      SignalRProvider.tsx
    router.tsx
  features/
    auth/
      api/
        authApi.ts
      components/
        LoginForm.tsx
        RoleToggle.tsx
        SignupForm.tsx
      pages/
        AuthPage.tsx
      schemas/
        authSchemas.test.ts
        authSchemas.ts
      store/
        authStore.ts
      types.ts
    doctor-dashboard/
      api/
        doctorApi.ts
      components/
        PatientDetailChrome.tsx
        PatientListFilters.tsx
        PatientListItem.tsx
        WardStatsCards.tsx
      hooks/
        usePatientList.ts
        useWardStats.ts
      pages/
        PatientDetailPage.tsx
        PatientListPage.tsx
      types.ts
    patient-dashboard/
      api/
        patientApi.ts
      components/
        AiReportCard.tsx
        AlertsTimeline.tsx
        EcgWaveform.tsx
        ecgWaveformMath.test.ts
        ecgWaveformMath.ts
        ElectrodeStatusCard.tsx
        HourlyStatusTimeline.tsx
        MedicalFileCard.tsx
        PatientDetailContent.tsx
        RiskStatusCard.test.tsx
        RiskStatusCard.tsx
        VitalsCard.tsx
        VitalsTrendChart.tsx
      constants.ts
      hooks/
        useLiveEcgStream.ts
        usePatientAlerts.ts
        usePatientProfile.ts
        usePatientVitals.ts
        usePatientVitalsHistory.ts
      pages/
        PatientDashboardPage.tsx
      types.ts
    settings/
      api/
        settingsApi.ts
      components/
        DevicePairingForm.tsx
        LanguagePreference.tsx
      pages/
        SettingsPage.tsx
  locales/
    ar/
      translation.json
    en/
      translation.json
  main.tsx
  routes/
    ProtectedRoute.tsx
  shared/
    components/
      LanguageToggle.tsx
      StatusPill.test.tsx
      StatusPill.tsx
      ThemeToggle.tsx
      ui/
        Badge.tsx
        Button.tsx
        Card.tsx
        Checkbox.tsx
        Input.tsx
        Modal.tsx
        PasswordInput.tsx
        SegmentedControl.tsx
        Select.tsx
        Tabs.tsx
        Textarea.tsx
    hooks/
      useDebouncedValue.ts
      useDirection.ts
      useJoinPatientGroup.ts
      useMediaQuery.ts
      useSignalR.tsx
    lib/
      apiClient.ts
      queryClient.ts
      signalrClient.ts
    store/
      themeStore.ts
    types/
      api.ts
    utils/
      cn.test.ts
      cn.ts
      formatters.test.ts
      formatters.ts
      guards.test.ts
      guards.ts
      jwt.ts
  styles/
    globals.css
  test/
    fixtures/
      samplePatient.ts
    setupTests.ts
  vite-env.d.ts
tsconfig.json
tsconfig.node.json
vite.config.ts
vitest.config.ts
```

### .

| File | Responsibility in plain words | Entry point or consumer |
|---|---|---|
| `.env.example` | Lists supported environment keys without deployed secrets. | Owning module or runtime tooling |
| `.eslintrc.cjs` | Declares .eslintrc.cjs settings or project metadata for CardioSense-Frontend-fixed. | Owning module or runtime tooling |
| `.gitignore` | Declares .gitignore settings or project metadata for CardioSense-Frontend-fixed. | Owning module or runtime tooling |
| `.prettierrc` | Declares .prettierrc settings or project metadata for CardioSense-Frontend-fixed. | Owning module or runtime tooling |
| `HANDOFF_REPORT.md` | Declares HANDOFF_REPORT.md settings or project metadata for CardioSense-Frontend-fixed. | Owning module or runtime tooling |
| `index.html` | Provides the browser entry document and mount node. | Owning module or runtime tooling |
| `package.json` | Declares npm scripts and dependency ranges. | Owning module or runtime tooling |
| `postcss.config.js` | Registers Tailwind CSS processing. | Owning module or runtime tooling |
| `README.md` | Declares README.md settings or project metadata for CardioSense-Frontend-fixed. | Owning module or runtime tooling |
| `tsconfig.json` | Defines strict application type checking. | Owning module or runtime tooling |
| `tsconfig.node.json` | Type-checks build and test configuration. | Owning module or runtime tooling |
| `vite.config.ts` | Configures the React build and local REST and WebSocket proxy. | Owning module or runtime tooling |
| `vitest.config.ts` | Configures test aliases and jsdom. | Owning module or runtime tooling |

### public/brand

| File | Responsibility in plain words | Entry point or consumer |
|---|---|---|
| `pulseforge-logo.jpeg` | Declares pulseforge-logo.jpeg settings or project metadata for brand. | Owning module or runtime tooling |

### public

| File | Responsibility in plain words | Entry point or consumer |
|---|---|---|
| `favicon.svg` | Declares favicon.svg settings or project metadata for public. | Owning module or runtime tooling |

### src/app

| File | Responsibility in plain words | Entry point or consumer |
|---|---|---|
| `App.tsx` | Defines App for the owning app module. Public symbols: `App`. | `HANDOFF_REPORT.md`, `src/main.tsx` |
| `router.tsx` | Defines router for the owning app module. Public symbols: `AppRouter`. | `src/app/App.tsx` |

### src/app/layouts

| File | Responsibility in plain words | Entry point or consumer |
|---|---|---|
| `AuthLayout.tsx` | Wraps nested routes with Auth Layout navigation and controls. Public symbols: `AuthLayout`. | `HANDOFF_REPORT.md`, `src/features/auth/pages/AuthPage.tsx` |
| `DoctorLayout.tsx` | Wraps nested routes with Doctor Layout navigation and controls. Public symbols: `DoctorLayout`. | `HANDOFF_REPORT.md`, `src/app/router.tsx` |
| `PatientLayout.tsx` | Wraps nested routes with Patient Layout navigation and controls. Public symbols: `PatientLayout`. | `HANDOFF_REPORT.md`, `src/app/router.tsx` |

### src/app/providers

| File | Responsibility in plain words | Entry point or consumer |
|---|---|---|
| `AuthProvider.tsx` | Wires Auth Provider into the React application lifecycle. Public symbols: `AuthProvider`. | `HANDOFF_REPORT.md`, `src/app/App.tsx` |
| `i18n.ts` | Wires i18n into the React application lifecycle. Public symbols: `i18n`. | `src/app/providers/I18nProvider.tsx` |
| `I18nProvider.tsx` | Wires I18n Provider into the React application lifecycle. Public symbols: `I18nProvider`. | `HANDOFF_REPORT.md`, `index.html` |
| `QueryProvider.tsx` | Wires Query Provider into the React application lifecycle. Public symbols: `QueryProvider`. | `HANDOFF_REPORT.md`, `src/app/App.tsx` |
| `SignalRProvider.tsx` | Wires Signal RProvider into the React application lifecycle. Public symbols: `SignalRProvider`. | `HANDOFF_REPORT.md`, `src/app/App.tsx` |

### src/features/auth/api

| File | Responsibility in plain words | Entry point or consumer |
|---|---|---|
| `authApi.ts` | Implements typed HTTP calls for auth Api. Public symbols: `login`, `signupPatient`, `signupDoctor`, `refreshSession`, `logout`. | `src/app/layouts/DoctorLayout.tsx`, `src/app/layouts/PatientLayout.tsx` |

### src/features/auth/components

| File | Responsibility in plain words | Entry point or consumer |
|---|---|---|
| `LoginForm.tsx` | Renders Login Form and forwards UI state and actions through component props. Public symbols: `LoginForm`. | `src/features/auth/pages/AuthPage.tsx`, `src/features/auth/schemas/authSchemas.ts` |
| `RoleToggle.tsx` | Renders Role Toggle and forwards UI state and actions through component props. Public symbols: `RoleToggle`. | `src/features/auth/components/SignupForm.tsx` |
| `SignupForm.tsx` | Renders Signup Form and forwards UI state and actions through component props. Public symbols: `SignupForm`. | `src/features/auth/pages/AuthPage.tsx`, `src/features/auth/schemas/authSchemas.ts` |

### src/features/auth/pages

| File | Responsibility in plain words | Entry point or consumer |
|---|---|---|
| `AuthPage.tsx` | Renders the Auth Page route and connects page-level data and actions. Public symbols: `AuthPage`. | `src/app/layouts/AuthLayout.tsx`, `src/app/router.tsx` |

### src/features/auth/schemas

| File | Responsibility in plain words | Entry point or consumer |
|---|---|---|
| `authSchemas.test.ts` | Exercises auth Schemas behavior with test fixtures. | Test runner |
| `authSchemas.ts` | Defines auth Schemas for the owning schemas module. Public symbols: `loginSchema`, `LoginFormValues`, `patientSignupSchema`, `PatientSignupFormValues`, `doctorSignupSchema`, `DoctorSignupFormValues`. | `src/features/auth/components/LoginForm.tsx`, `src/features/auth/components/SignupForm.tsx` |

### src/features/auth/store

| File | Responsibility in plain words | Entry point or consumer |
|---|---|---|
| `authStore.ts` | Defines auth Store for the owning store module. Public symbols: `AuthStatus`, `useAuthStore`. | `src/app/layouts/DoctorLayout.tsx`, `src/app/layouts/PatientLayout.tsx` |

### src/features/auth

| File | Responsibility in plain words | Entry point or consumer |
|---|---|---|
| `types.ts` | Defines types for the owning auth module. Public symbols: `AuthTab`, `SignupRole`. | `src/features/auth/components/RoleToggle.tsx`, `src/features/auth/components/SignupForm.tsx` |

### src/features/doctor-dashboard/api

| File | Responsibility in plain words | Entry point or consumer |
|---|---|---|
| `doctorApi.ts` | Implements typed HTTP calls for doctor Api. Public symbols: `getDoctorProfile`, `updateMyDoctorProfile`, `getMyPatients`, `getWardStats`, `addPatient`, `logManualEvent`. | `src/app/providers/AuthProvider.tsx`, `src/features/doctor-dashboard/components/PatientDetailChrome.tsx` |

### src/features/doctor-dashboard/components

| File | Responsibility in plain words | Entry point or consumer |
|---|---|---|
| `PatientDetailChrome.tsx` | Renders Patient Detail Chrome and forwards UI state and actions through component props. Public symbols: `PatientDetailChrome`. | `HANDOFF_REPORT.md`, `src/features/doctor-dashboard/pages/PatientDetailPage.tsx` |
| `PatientListFilters.tsx` | Renders Patient List Filters and forwards UI state and actions through component props. Public symbols: `PatientListFilters`. | `src/features/doctor-dashboard/pages/PatientListPage.tsx` |
| `PatientListItem.tsx` | Renders Patient List Item and forwards UI state and actions through component props. Public symbols: `PatientListItem`. | `src/features/doctor-dashboard/api/doctorApi.ts`, `src/features/doctor-dashboard/pages/PatientListPage.tsx` |
| `WardStatsCards.tsx` | Renders Ward Stats Cards and forwards UI state and actions through component props. Public symbols: `WardStatsCards`. | `src/features/doctor-dashboard/pages/PatientListPage.tsx` |

### src/features/doctor-dashboard/hooks

| File | Responsibility in plain words | Entry point or consumer |
|---|---|---|
| `usePatientList.ts` | Provides use Patient List to its consumers and owns associated query, state or subscription lifecycle. Public symbols: `usePatientList`. | `src/features/doctor-dashboard/pages/PatientListPage.tsx` |
| `useWardStats.ts` | Provides use Ward Stats to its consumers and owns associated query, state or subscription lifecycle. Public symbols: `useWardStats`. | `src/app/layouts/DoctorLayout.tsx`, `src/features/doctor-dashboard/pages/PatientListPage.tsx` |

### src/features/doctor-dashboard/pages

| File | Responsibility in plain words | Entry point or consumer |
|---|---|---|
| `PatientDetailPage.tsx` | Renders the Patient Detail Page route and connects page-level data and actions. Public symbols: `PatientDetailPage`. | `src/app/router.tsx`, `src/features/patient-dashboard/hooks/useLiveEcgStream.ts` |
| `PatientListPage.tsx` | Renders the Patient List Page route and connects page-level data and actions. Public symbols: `PatientListPage`. | `src/app/router.tsx` |

### src/features/doctor-dashboard

| File | Responsibility in plain words | Entry point or consumer |
|---|---|---|
| `types.ts` | Defines types for the owning doctor-dashboard module. Public symbols: `PatientListQueryParams`, `PATIENT_LIST_PAGE_SIZE`. | `src/features/doctor-dashboard/api/doctorApi.ts`, `src/features/doctor-dashboard/hooks/usePatientList.ts` |

### src/features/patient-dashboard/api

| File | Responsibility in plain words | Entry point or consumer |
|---|---|---|
| `patientApi.ts` | Implements typed HTTP calls for patient Api. Public symbols: `getPatientProfile`, `updateMyProfile`, `getPatientVitalsHistory`, `getPatientAlerts`, `getPatientAiReport`. | `src/app/providers/AuthProvider.tsx`, `src/features/patient-dashboard/components/AiReportCard.tsx` |

### src/features/patient-dashboard/components

| File | Responsibility in plain words | Entry point or consumer |
|---|---|---|
| `AiReportCard.tsx` | Renders Ai Report Card and forwards UI state and actions through component props. Public symbols: `AiReportCard`. | `src/features/patient-dashboard/components/PatientDetailContent.tsx` |
| `AlertsTimeline.tsx` | Renders Alerts Timeline and forwards UI state and actions through component props. Public symbols: `AlertsTimeline`. | `src/features/patient-dashboard/components/PatientDetailContent.tsx` |
| `EcgWaveform.tsx` | Renders Ecg Waveform and forwards UI state and actions through component props. Public symbols: `EcgWaveform`. | `HANDOFF_REPORT.md`, `src/features/doctor-dashboard/pages/PatientDetailPage.tsx` |
| `ecgWaveformMath.test.ts` | Exercises ecg Waveform Math behavior with test fixtures. | Test runner |
| `ecgWaveformMath.ts` | Renders ecg Waveform Math and forwards UI state and actions through component props. Public symbols: `idleEcgValueAt`, `generateIdleWindow`, `computeIdleLandmarkTimes`, `unwrapRingBuffer`, `BeatLandmarkIndices`, `detectBeatLandmarks`. | `src/features/patient-dashboard/components/EcgWaveform.tsx`, `src/features/patient-dashboard/components/ecgWaveformMath.test.ts` |
| `ElectrodeStatusCard.tsx` | Renders Electrode Status Card and forwards UI state and actions through component props. Public symbols: `ElectrodeStatusCard`. | `src/features/patient-dashboard/components/PatientDetailContent.tsx` |
| `HourlyStatusTimeline.tsx` | Renders Hourly Status Timeline and forwards UI state and actions through component props. Public symbols: `HourlyStatusTimeline`. | `src/features/patient-dashboard/components/PatientDetailContent.tsx` |
| `MedicalFileCard.tsx` | Renders Medical File Card and forwards UI state and actions through component props. Public symbols: `MedicalFileCard`. | `src/features/patient-dashboard/components/PatientDetailContent.tsx` |
| `PatientDetailContent.tsx` | Renders Patient Detail Content and forwards UI state and actions through component props. Public symbols: `PatientDetailContent`. | `HANDOFF_REPORT.md`, `src/features/doctor-dashboard/components/PatientDetailChrome.tsx` |
| `RiskStatusCard.test.tsx` | Exercises Risk Status Card behavior with test fixtures. | Test runner |
| `RiskStatusCard.tsx` | Renders Risk Status Card and forwards UI state and actions through component props. Public symbols: `RiskStatusCard`. | `src/features/patient-dashboard/components/PatientDetailContent.tsx`, `src/features/patient-dashboard/components/RiskStatusCard.test.tsx` |
| `VitalsCard.tsx` | Renders Vitals Card and forwards UI state and actions through component props. Public symbols: `VitalsCard`. | `src/features/patient-dashboard/components/PatientDetailContent.tsx` |
| `VitalsTrendChart.tsx` | Renders Vitals Trend Chart and forwards UI state and actions through component props. Public symbols: `VitalsTrendChart`. | `HANDOFF_REPORT.md`, `src/features/patient-dashboard/components/PatientDetailContent.tsx` |

### src/features/patient-dashboard

| File | Responsibility in plain words | Entry point or consumer |
|---|---|---|
| `constants.ts` | Defines constants for the owning patient-dashboard module. Public symbols: `ECG_SAMPLE_RATE_HZ`, `ECG_WINDOW_SECONDS`, `ECG_WINDOW_SIZE`, `ECG_CALIBRATION`, `ECG_GRID_MINOR_PX`, `ECG_GRID_MAJOR_EVERY`. | `src/features/patient-dashboard/components/EcgWaveform.tsx`, `src/features/patient-dashboard/hooks/useLiveEcgStream.ts` |
| `types.ts` | Defines types for the owning patient-dashboard module. Public symbols: `VitalsHistoryRange`, `LiveStreamStatus`. | `src/features/patient-dashboard/api/patientApi.ts`, `src/features/patient-dashboard/components/VitalsTrendChart.tsx` |

### src/features/patient-dashboard/hooks

| File | Responsibility in plain words | Entry point or consumer |
|---|---|---|
| `useLiveEcgStream.ts` | Provides use Live Ecg Stream to its consumers and owns associated query, state or subscription lifecycle. Public symbols: `LiveEcgStream`, `useLiveEcgStream`. | `src/features/patient-dashboard/components/EcgWaveform.tsx` |
| `usePatientAlerts.ts` | Provides use Patient Alerts to its consumers and owns associated query, state or subscription lifecycle. Public symbols: `usePatientAlerts`. | `src/features/doctor-dashboard/pages/PatientDetailPage.tsx`, `src/features/patient-dashboard/pages/PatientDashboardPage.tsx` |
| `usePatientProfile.ts` | Provides use Patient Profile to its consumers and owns associated query, state or subscription lifecycle. Public symbols: `usePatientProfile`. | `src/features/doctor-dashboard/pages/PatientDetailPage.tsx`, `src/features/patient-dashboard/pages/PatientDashboardPage.tsx` |
| `usePatientVitals.ts` | Provides use Patient Vitals to its consumers and owns associated query, state or subscription lifecycle. Public symbols: `LiveVitalsState`, `usePatientVitals`. | `src/features/doctor-dashboard/components/PatientDetailChrome.tsx`, `src/features/doctor-dashboard/pages/PatientDetailPage.tsx` |
| `usePatientVitalsHistory.ts` | Provides use Patient Vitals History to its consumers and owns associated query, state or subscription lifecycle. Public symbols: `usePatientVitalsHistory`. | `src/features/patient-dashboard/components/VitalsTrendChart.tsx` |

### src/features/patient-dashboard/pages

| File | Responsibility in plain words | Entry point or consumer |
|---|---|---|
| `PatientDashboardPage.tsx` | Renders the Patient Dashboard Page route and connects page-level data and actions. Public symbols: `PatientDashboardPage`. | `HANDOFF_REPORT.md`, `src/app/router.tsx` |

### src/features/settings/api

| File | Responsibility in plain words | Entry point or consumer |
|---|---|---|
| `settingsApi.ts` | Implements typed HTTP calls for settings Api. Public symbols: `pairDevice`, `getDeviceStatus`. | `src/features/settings/components/DevicePairingForm.tsx` |

### src/features/settings/components

| File | Responsibility in plain words | Entry point or consumer |
|---|---|---|
| `DevicePairingForm.tsx` | Renders Device Pairing Form and forwards UI state and actions through component props. Public symbols: `DevicePairingForm`. | `src/features/settings/pages/SettingsPage.tsx` |
| `LanguagePreference.tsx` | Renders Language Preference and forwards UI state and actions through component props. Public symbols: `LanguagePreference`. | `src/features/settings/pages/SettingsPage.tsx` |

### src/features/settings/pages

| File | Responsibility in plain words | Entry point or consumer |
|---|---|---|
| `SettingsPage.tsx` | Renders the Settings Page route and connects page-level data and actions. Public symbols: `SettingsPage`. | `src/app/router.tsx` |

### src/locales/ar

| File | Responsibility in plain words | Entry point or consumer |
|---|---|---|
| `translation.json` | Provides the ar translation dictionary for i18next. | Owning module or runtime tooling |

### src/locales/en

| File | Responsibility in plain words | Entry point or consumer |
|---|---|---|
| `translation.json` | Provides the en translation dictionary for i18next. | Owning module or runtime tooling |

### src

| File | Responsibility in plain words | Entry point or consumer |
|---|---|---|
| `main.tsx` | Defines main for the owning src module. | Owning module or runtime tooling |
| `vite-env.d.ts` | Defines vite-env.d for the owning src module. | Owning module or runtime tooling |

### src/routes

| File | Responsibility in plain words | Entry point or consumer |
|---|---|---|
| `ProtectedRoute.tsx` | Defines Protected Route for the owning routes module. Public symbols: `ProtectedRoute`. | `HANDOFF_REPORT.md`, `README.md` |

### src/shared/components

| File | Responsibility in plain words | Entry point or consumer |
|---|---|---|
| `LanguageToggle.tsx` | Renders Language Toggle and forwards UI state and actions through component props. Public symbols: `LanguageToggle`. | `HANDOFF_REPORT.md`, `src/app/layouts/AuthLayout.tsx` |
| `StatusPill.test.tsx` | Exercises Status Pill behavior with test fixtures. | Test runner |
| `StatusPill.tsx` | Renders Status Pill and forwards UI state and actions through component props. Public symbols: `StatusPill`. | `HANDOFF_REPORT.md`, `README.md` |
| `ThemeToggle.tsx` | Renders Theme Toggle and forwards UI state and actions through component props. Public symbols: `ThemeToggle`. | `HANDOFF_REPORT.md`, `src/app/layouts/AuthLayout.tsx` |

### src/shared/components/ui

| File | Responsibility in plain words | Entry point or consumer |
|---|---|---|
| `Badge.tsx` | Renders Badge and forwards UI state and actions through component props. Public symbols: `BadgeProps`, `Badge`. | `HANDOFF_REPORT.md`, `src/app/layouts/AuthLayout.tsx` |
| `Button.tsx` | Renders Button and forwards UI state and actions through component props. Public symbols: `buttonVariants`, `ButtonProps`, `Button`. | `src/features/auth/components/LoginForm.tsx`, `src/features/auth/components/SignupForm.tsx` |
| `Card.tsx` | Renders Card and forwards UI state and actions through component props. Public symbols: `Card`, `CardHeader`, `CardTitle`. | `src/features/doctor-dashboard/components/PatientListItem.tsx`, `src/features/doctor-dashboard/components/WardStatsCards.tsx` |
| `Checkbox.tsx` | Renders Checkbox and forwards UI state and actions through component props. Public symbols: `CheckboxProps`, `Checkbox`. | `src/features/auth/components/SignupForm.tsx` |
| `Input.tsx` | Renders Input and forwards UI state and actions through component props. Public symbols: `InputProps`, `Input`. | `src/features/auth/components/LoginForm.tsx`, `src/features/auth/components/SignupForm.tsx` |
| `Modal.tsx` | Renders Modal and forwards UI state and actions through component props. Public symbols: `Modal`. | `HANDOFF_REPORT.md`, `src/features/doctor-dashboard/components/PatientDetailChrome.tsx` |
| `PasswordInput.tsx` | Renders Password Input and forwards UI state and actions through component props. Public symbols: `PasswordInput`. | `src/features/auth/components/LoginForm.tsx`, `src/features/auth/components/SignupForm.tsx` |
| `SegmentedControl.tsx` | Renders Segmented Control and forwards UI state and actions through component props. Public symbols: `SegmentedOption`, `SegmentedControl`. | `src/features/auth/components/RoleToggle.tsx`, `src/features/doctor-dashboard/components/PatientListFilters.tsx` |
| `Select.tsx` | Renders Select and forwards UI state and actions through component props. Public symbols: `SelectOption`, `SelectProps`, `Select`. | `src/features/auth/components/SignupForm.tsx`, `src/features/doctor-dashboard/components/PatientDetailChrome.tsx` |
| `Tabs.tsx` | Renders Tabs and forwards UI state and actions through component props. Public symbols: `TabOption`, `Tabs`. | `src/features/auth/pages/AuthPage.tsx`, `src/features/patient-dashboard/components/AiReportCard.tsx` |
| `Textarea.tsx` | Renders Textarea and forwards UI state and actions through component props. Public symbols: `TextareaProps`, `Textarea`. | `src/features/auth/components/SignupForm.tsx`, `src/features/doctor-dashboard/components/PatientDetailChrome.tsx` |

### src/shared/hooks

| File | Responsibility in plain words | Entry point or consumer |
|---|---|---|
| `useDebouncedValue.ts` | Provides use Debounced Value to its consumers and owns associated query, state or subscription lifecycle. Public symbols: `useDebouncedValue`. | `src/features/doctor-dashboard/hooks/usePatientList.ts` |
| `useDirection.ts` | Provides use Direction to its consumers and owns associated query, state or subscription lifecycle. Public symbols: `TextDirection`, `useDirection`. | Owning module or runtime tooling |
| `useJoinPatientGroup.ts` | Provides use Join Patient Group to its consumers and owns associated query, state or subscription lifecycle. Public symbols: `useJoinPatientGroup`. | `src/features/doctor-dashboard/pages/PatientDetailPage.tsx` |
| `useMediaQuery.ts` | Provides use Media Query to its consumers and owns associated query, state or subscription lifecycle. Public symbols: `useMediaQuery`. | Owning module or runtime tooling |
| `useSignalR.tsx` | Provides use Signal R to its consumers and owns associated query, state or subscription lifecycle. Public symbols: `SignalRConnectionState`, `SignalRContextValue`, `SignalRContext`, `useSignalR`. | `src/app/layouts/DoctorLayout.tsx`, `src/app/providers/SignalRProvider.tsx` |

### src/shared/lib

| File | Responsibility in plain words | Entry point or consumer |
|---|---|---|
| `apiClient.ts` | Defines api Client for the owning lib module. Public symbols: `apiClient`, `configureApiClientAuth`. | `src/app/providers/AuthProvider.tsx`, `src/features/auth/api/authApi.ts` |
| `queryClient.ts` | Defines query Client for the owning lib module. Public symbols: `queryClient`. | `src/app/providers/QueryProvider.tsx`, `src/features/auth/store/authStore.ts` |
| `signalrClient.ts` | Defines signalr Client for the owning lib module. Public symbols: `createHubConnection`. | `src/app/providers/SignalRProvider.tsx` |

### src/shared/store

| File | Responsibility in plain words | Entry point or consumer |
|---|---|---|
| `themeStore.ts` | Defines theme Store for the owning store module. Public symbols: `ThemeMode`, `useThemeStore`, `applyThemeToDocument`. | `src/main.tsx`, `src/shared/components/ThemeToggle.tsx` |

### src/shared/types

| File | Responsibility in plain words | Entry point or consumer |
|---|---|---|
| `api.ts` | Defines api for the owning types module. Public symbols: `UserRole`, `ConnectionMode`, `RiskStatusLevel`, `AlertSeverity`, `MedicalHistoryEntryType`, `Gender`. | `src/app/providers/AuthProvider.tsx`, `src/features/auth/api/authApi.ts` |

### src/shared/utils

| File | Responsibility in plain words | Entry point or consumer |
|---|---|---|
| `cn.test.ts` | Exercises cn behavior with test fixtures. | Test runner |
| `cn.ts` | Defines cn for the owning utils module. Public symbols: `cn`. | `src/app/layouts/DoctorLayout.tsx`, `src/features/doctor-dashboard/components/PatientListItem.tsx` |
| `formatters.test.ts` | Exercises formatters behavior with test fixtures. | Test runner |
| `formatters.ts` | Defines formatters for the owning utils module. Public symbols: `formatBpm`, `formatPercent`, `formatMs`, `formatTimestamp`, `formatHour`, `formatRelativeTime`. | `src/features/doctor-dashboard/components/PatientListItem.tsx`, `src/features/patient-dashboard/components/AiReportCard.tsx` |
| `guards.test.ts` | Exercises guards behavior with test fixtures. | Test runner |
| `guards.ts` | Defines guards for the owning utils module. Public symbols: `isDoctor`, `isPatient`. | `src/features/settings/pages/SettingsPage.tsx`, `src/shared/utils/guards.test.ts` |
| `jwt.ts` | Defines jwt for the owning utils module. Public symbols: `DecodedAccessToken`, `decodeJwtPayload`. | `src/app/providers/AuthProvider.tsx` |

### src/styles

| File | Responsibility in plain words | Entry point or consumer |
|---|---|---|
| `globals.css` | Defines globals for the owning styles module. | Owning module or runtime tooling |

### src/test/fixtures

| File | Responsibility in plain words | Entry point or consumer |
|---|---|---|
| `samplePatient.ts` | Defines sample Patient for the owning fixtures module. Public symbols: `sampleProfileFixture`, `sampleAlertFixture`, `samplePatientListItemFixture`. | Owning module or runtime tooling |

### src/test

| File | Responsibility in plain words | Entry point or consumer |
|---|---|---|
| `setupTests.ts` | Defines setup Tests for the owning test module. | Test runner |

## Backend

Root: `back-end/back/cardiosense-backend`. 260 listed files and 260 inventory entries.

```text
.dockerignore
.editorconfig
.env.example
.gitignore
CardioSense.sln
Directory.Build.props
Directory.Packages.props
docker-compose.yml
Dockerfile
mosquitto/
  generate-dev-certs.sh
  mosquitto.conf
src/
  CardioSense.Api/
    appsettings.Development.json
    appsettings.json
    Authentication/
      CurrentUser.cs
      CurrentUserHubFilter.cs
      JwtBearerSetup.cs
      RefreshCookie.cs
    Authorization/
      AuthorizationPolicies.cs
      PatientAccessFilter.cs
      PatientAccessHandler.cs
      PatientAccessRequirement.cs
    CardioSense.Api.csproj
    Contracts/
      PatientRequests.cs
    DependencyInjection.cs
    Endpoints/
      ApiRoutes.cs
      AuthEndpoints.cs
      DeviceEndpoints.cs
      DoctorEndpoints.cs
      EndpointExtensions.cs
      HealthEndpoints.cs
      IEndpointGroup.cs
      MonitoringHubEndpoints.cs
      OpenApiEndpoints.cs
      PatientEndpoints.cs
    Http/
      AllowedOriginFilter.cs
      ApiErrors.cs
      ResultHttpExtensions.cs
    Middleware/
      AuditLoggingMiddleware.cs
      GlobalExceptionHandler.cs
      PatientAuditMetadata.cs
      SecurityHeadersMiddleware.cs
    Options/
      ApiCorsOptions.cs
      DatabaseInitialization.cs
      DatabaseOptions.cs
      RateLimitOptions.cs
      RefreshCookieOptions.cs
    Program.cs
    Properties/
      launchSettings.json
    WebApplicationExtensions.cs
  CardioSense.Application/
    Auth/
      Commands/
        Login/
          LoginCommand.cs
          LoginHandler.cs
          LoginValidator.cs
        Logout/
          LogoutCommand.cs
          LogoutHandler.cs
        RefreshToken/
          RefreshTokenCommand.cs
          RefreshTokenHandler.cs
        RegisterDoctor/
          RegisterDoctorCommand.cs
          RegisterDoctorHandler.cs
          RegisterDoctorValidator.cs
        RegisterPatient/
          RegisterPatientCommand.cs
          RegisterPatientHandler.cs
          RegisterPatientValidator.cs
      Common/
        AuthDtos.cs
        AuthSession.cs
        SessionIssuer.cs
    CardioSense.Application.csproj
    Common/
      Behaviors/
        AuthorizationBehavior.cs
        ResultFailureFactory.cs
        ValidationBehavior.cs
      Contracts/
        ContractLiterals.cs
      Dtos/
        AiReportDto.cs
        AlertDto.cs
        ClinicalDtos.cs
        DeviceSummaryDto.cs
        DoctorProfileDto.cs
        PagedResult.cs
        PatientListItemDto.cs
        PatientProfileDto.cs
        RealtimePayloads.cs
        WardStatsDto.cs
      Errors/
        ApplicationErrors.cs
      Interfaces/
        IAppDbContext.cs
        ICurrentUser.cs
        IIdentityService.cs
        IPatientAccessChecker.cs
        IPatientScopedRequest.cs
        IRealtimeNotifier.cs
        IRiskAssessmentService.cs
        ITokenService.cs
      Mappings/
        AlertMappings.cs
        ClinicalMappings.cs
        DeviceMappings.cs
        ProfileMappings.cs
        StatusLabels.cs
      Models/
        AiModels.cs
        IdentityModels.cs
        TokenModels.cs
      Options/
        MonitoringOptions.cs
      Security/
        PatientAccessChecker.cs
      Validation/
        CommonRules.cs
    DependencyInjection.cs
    Doctors/
      Commands/
        UpdateDoctorProfile/
          UpdateDoctorProfileCommand.cs
          UpdateDoctorProfileHandler.cs
          UpdateDoctorProfileValidator.cs
      Common/
        DoctorCaseload.cs
        PatientListItemAssembler.cs
      Queries/
        GetDoctorProfile/
          GetDoctorProfileHandler.cs
          GetDoctorProfileQuery.cs
        GetMyPatients/
          GetMyPatientsHandler.cs
          GetMyPatientsQuery.cs
          GetMyPatientsValidator.cs
        GetWardStats/
          GetWardStatsHandler.cs
          GetWardStatsQuery.cs
    Patients/
      Commands/
        AssignDoctor/
          AssignDoctorCommand.cs
          AssignDoctorHandler.cs
          AssignDoctorValidator.cs
        FreezeWaveform/
          FreezeWaveformCommand.cs
          FreezeWaveformHandler.cs
        LogManualEvent/
          LogManualEventCommand.cs
          LogManualEventHandler.cs
          LogManualEventValidator.cs
        PairDevice/
          PairDeviceCommand.cs
          PairDeviceHandler.cs
          PairDeviceValidator.cs
        ResolveAlert/
          ResolveAlertCommand.cs
          ResolveAlertHandler.cs
          ResolveAlertValidator.cs
        UpdatePatientProfile/
          UpdatePatientProfileCommand.cs
          UpdatePatientProfileHandler.cs
          UpdatePatientProfileValidator.cs
      Common/
        PatientProfileAssembler.cs
      Queries/
        GetDeviceStatus/
          GetDeviceStatusHandler.cs
          GetDeviceStatusQuery.cs
          GetDeviceStatusValidator.cs
        GetPatientAiReport/
          GetPatientAiReportHandler.cs
          GetPatientAiReportQuery.cs
          GetPatientAiReportValidator.cs
        GetPatientAlerts/
          GetPatientAlertsHandler.cs
          GetPatientAlertsQuery.cs
        GetPatientProfile/
          GetPatientProfileHandler.cs
          GetPatientProfileQuery.cs
        GetPatientVitalsHistory/
          GetPatientVitalsHistoryHandler.cs
          GetPatientVitalsHistoryQuery.cs
          GetPatientVitalsHistoryValidator.cs
    Telemetry/
      Commands/
        ProcessDeviceStatus/
          ProcessDeviceStatusCommand.cs
          ProcessDeviceStatusHandler.cs
          ProcessDeviceStatusValidator.cs
        ProcessTelemetry/
          ProcessTelemetryCommand.cs
          ProcessTelemetryHandler.cs
          ProcessTelemetryValidator.cs
  CardioSense.Domain/
    CardioSense.Domain.csproj
    Common/
      DomainErrors.cs
      Entity.cs
      Error.cs
      ErrorType.cs
      Result.cs
    Entities/
      Alert.cs
      AuditLogEntry.cs
      Device.cs
      Doctor.cs
      DoctorPatientAssignment.cs
      EcgFeatureSet.cs
      ElectrodeVerification.cs
      MedicalHistoryEntry.cs
      Patient.cs
      RefreshToken.cs
      RiskAssessment.cs
      VitalsSample.cs
    Enums/
      AlertSeverity.cs
      AlertType.cs
      ConnectionMode.cs
      ElectrodePlacementStatus.cs
      Gender.cs
      MedicalHistoryEntryType.cs
      RiskStatusLevel.cs
      UserRole.cs
    Exceptions/
      DomainException.cs
    Interfaces/
      HeartRateRange.cs
      IVitalsTimeSeriesStore.cs
  CardioSense.Infrastructure/
    AiClient/
      AiContracts.cs
      AiErrors.cs
      AiServiceOptions.cs
      HttpRiskAssessmentService.cs
    CardioSense.Infrastructure.csproj
    DependencyInjection.cs
    Identity/
      ApplicationUser.cs
      IdentityErrors.cs
      IdentityService.cs
      JwtClaimNames.cs
      JwtOptions.cs
      JwtTokenService.cs
    Ingestion/
      IngestionErrors.cs
      MqttIngestionService.cs
      MqttOptions.cs
      TelemetryMessageParser.cs
    Persistence/
      AppDbContext.cs
      AppDbContextFactory.cs
      Configurations/
        AlertConfiguration.cs
        ApplicationUserConfiguration.cs
        AuditLogEntryConfiguration.cs
        DeviceConfiguration.cs
        DoctorConfiguration.cs
        DoctorPatientAssignmentConfiguration.cs
        EcgFeatureSetConfiguration.cs
        ElectrodeVerificationConfiguration.cs
        EntityConfigurationExtensions.cs
        MedicalHistoryEntryConfiguration.cs
        PatientConfiguration.cs
        RefreshTokenConfiguration.cs
        RiskAssessmentConfiguration.cs
        VitalsSampleConfiguration.cs
      DatabaseInitializer.cs
      UniqueViolation.cs
    Realtime/
      MonitoringHub.cs
      MonitoringNames.cs
      SignalRRealtimeNotifier.cs
    TimeSeries/
      SqlVitalsTimeSeriesStore.cs
tests/
  CardioSense.Api.IntegrationTests/
    CardioSense.Api.IntegrationTests.csproj
    Scenarios/
      AccessControlTests.cs
      AuthFlowTests.cs
      DevicePairingTests.cs
      HealthAndErrorShapeTests.cs
      MonitoringHubTests.cs
    Support/
      Accounts.cs
      ApiCollection.cs
      ApiFactory.cs
      StubAiService.cs
  CardioSense.Application.Tests/
    CardioSense.Application.Tests.csproj
    Entities/
      AlertTests.cs
      DeviceTests.cs
      PatientTests.cs
      RefreshTokenTests.cs
      ResultTests.cs
      RiskAssessmentTests.cs
      VitalsSampleTests.cs
    Pipeline/
      AuthorizationPipelineTests.cs
      GetMyPatientsTests.cs
      PairDeviceTests.cs
      ProcessTelemetryTests.cs
      RefreshTokenPipelineTests.cs
      RegisterPatientTests.cs
      ValidationPipelineTests.cs
    Support/
      ApplicationTestHost.cs
      FakeCurrentUser.cs
      FixedTimeProvider.cs
      InMemoryVitalsStore.cs
      RecordingNotifier.cs
      Seed.cs
      StubIdentityService.cs
      StubRiskAssessmentService.cs
      StubTokenService.cs
      TestDbContext.cs
  CardioSense.Infrastructure.Tests/
    AiClient/
      HttpRiskAssessmentServiceTests.cs
    CardioSense.Infrastructure.Tests.csproj
    Identity/
      IdentityErrorsTests.cs
      JwtTokenServiceTests.cs
    Ingestion/
      TelemetryMessageParserTests.cs
    Persistence/
      PersistenceModelTests.cs
      UniqueViolationTests.cs
    Support/
      FixedTimeProvider.cs
```

### .

| File | Responsibility in plain words | Entry point or consumer |
|---|---|---|
| `.dockerignore` | Declares .dockerignore settings or project metadata for cardiosense-backend. | Owning module or runtime tooling |
| `.editorconfig` | Declares .editorconfig settings or project metadata for cardiosense-backend. | Owning module or runtime tooling |
| `.env.example` | Lists supported environment keys without deployed secrets. | Owning module or runtime tooling |
| `.gitignore` | Declares .gitignore settings or project metadata for cardiosense-backend. | Owning module or runtime tooling |
| `CardioSense.sln` | Declares CardioSense.sln settings or project metadata for cardiosense-backend. | Owning module or runtime tooling |
| `Directory.Build.props` | Declares Directory.Build.props settings or project metadata for cardiosense-backend. | Owning module or runtime tooling |
| `Directory.Packages.props` | Declares Directory.Packages.props settings or project metadata for cardiosense-backend. | Owning module or runtime tooling |
| `docker-compose.yml` | Orchestrates SQL, certificate generation, MQTT, API and AI services. | Owning module or runtime tooling |
| `Dockerfile` | Builds the service image and defines its entry command. | Owning module or runtime tooling |

### mosquitto

| File | Responsibility in plain words | Entry point or consumer |
|---|---|---|
| `generate-dev-certs.sh` | Declares generate-dev-certs.sh settings or project metadata for mosquitto. | Owning module or runtime tooling |
| `mosquitto.conf` | Declares mosquitto.conf settings or project metadata for mosquitto. | Owning module or runtime tooling |

### src/CardioSense.Api

| File | Responsibility in plain words | Entry point or consumer |
|---|---|---|
| `appsettings.Development.json` | Declares appsettings.Development.json settings or project metadata for CardioSense.Api. | Owning module or runtime tooling |
| `appsettings.json` | Declares appsettings.json settings or project metadata for CardioSense.Api. | Owning module or runtime tooling |
| `CardioSense.Api.csproj` | Declares CardioSense.Api.csproj settings or project metadata for CardioSense.Api. | Owning module or runtime tooling |
| `DependencyInjection.cs` | AddApi(): registers everything the HTTP host itself needs, in one call from Program.cs. Grouped by concern - options, authentication/authorization, browser access (CORS, rate limiting, | `Directory.Packages.props`, `src/CardioSense.Api/Endpoints/EndpointExtensions.cs` |
| `Program.cs` | Entry point and composition root of the CardioSense API. Registers the three layers (Application first, because it registers the shared TimeProvider that | `src/CardioSense.Api/DependencyInjection.cs`, `src/CardioSense.Api/Endpoints/EndpointExtensions.cs` |
| `WebApplicationExtensions.cs` | The request pipeline (UseApi) and the start-up database step (InitializeDatabaseAsync). Middleware order matters and is written down here once: errors are caught first, security headers | Owning module or runtime tooling |

### src/CardioSense.Api/Authentication

| File | Responsibility in plain words | Entry point or consumer |
|---|---|---|
| `CurrentUser.cs` | Implements ICurrentUser: who is calling, read from the validated access-token claims. Application and Infrastructure never touch HttpContext; they ask ICurrentUser. Claims are read by | `src/CardioSense.Api/Authentication/CurrentUserHubFilter.cs`, `src/CardioSense.Api/DependencyInjection.cs` |
| `CurrentUserHubFilter.cs` | Makes ICurrentUser work inside SignalR hub methods. MonitoringHub.JoinPatientGroup asks IPatientAccessChecker, which asks ICurrentUser. Inside a hub | `src/CardioSense.Api/DependencyInjection.cs`, `tests/CardioSense.Api.IntegrationTests/Scenarios/MonitoringHubTests.cs` |
| `JwtBearerSetup.cs` | Configures how incoming access tokens are validated. Validation mirrors exactly what JwtTokenService writes: same issuer, audience, HS256 key and the | `src/CardioSense.Api/Authentication/CurrentUser.cs`, `src/CardioSense.Api/DependencyInjection.cs` |
| `RefreshCookie.cs` | Writes, reads and clears the httpOnly cookie that carries the refresh token. The refresh token never appears in a JSON body or in JavaScript-readable storage (the frontend | `src/CardioSense.Api/DependencyInjection.cs`, `src/CardioSense.Api/Endpoints/AuthEndpoints.cs` |

### src/CardioSense.Api/Authorization

| File | Responsibility in plain words | Entry point or consumer |
|---|---|---|
| `AuthorizationPolicies.cs` | Names of the role policies used by the endpoints. Role-level rules ("only a patient", "only a doctor") are declared once as named policies and | `src/CardioSense.Api/Endpoints/DoctorEndpoints.cs`, `src/CardioSense.Api/Endpoints/PatientEndpoints.cs` |
| `PatientAccessFilter.cs` | Endpoint filter that applies PatientAccessRequirement to every route containing {id} (a patient). Declared once on the /api/patients/{id} route group, so no individual endpoint can forget the | `src/CardioSense.Api/Endpoints/PatientEndpoints.cs` |
| `PatientAccessHandler.cs` | Thin ASP.NET Core authorization adapter over IPatientAccessChecker. "Can this caller see this patient?" is decided in exactly one place - IPatientAccessChecker in | `src/CardioSense.Api/Authorization/PatientAccessRequirement.cs`, `src/CardioSense.Api/DependencyInjection.cs` |
| `PatientAccessRequirement.cs` | Marker requirement: "the caller may see this particular patient". Carries no data; the patient id is the resource being authorized, and PatientAccessHandler decides. | `src/CardioSense.Api/Authorization/AuthorizationPolicies.cs`, `src/CardioSense.Api/Authorization/PatientAccessFilter.cs` |

### src/CardioSense.Api/Contracts

| File | Responsibility in plain words | Entry point or consumer |
|---|---|---|
| `PatientRequests.cs` | JSON request bodies of the patient endpoints that cannot bind straight to an Application command. The command carries the patient id and the caller's identity, which come from the URL or the | Owning module or runtime tooling |

### src/CardioSense.Api/Endpoints

| File | Responsibility in plain words | Entry point or consumer |
|---|---|---|
| `ApiRoutes.cs` | Route and policy names that more than one file needs. The SignalR hub path is used by the endpoint that maps the hub and by the JWT setup that accepts a | `src/CardioSense.Api/Authentication/JwtBearerSetup.cs`, `src/CardioSense.Api/DependencyInjection.cs` |
| `AuthEndpoints.cs` | /api/auth - sign up (patient or doctor), log in, refresh the session, log out. The Application returns an AuthSession holding both tokens. The endpoint splits it: the raw refresh | Owning module or runtime tooling |
| `DeviceEndpoints.cs` | /api/devices - pair a device to a patient and read a device's status card. Open to any signed-in user because both roles use it (a patient pairs their own device, a doctor | Owning module or runtime tooling |
| `DoctorEndpoints.cs` | /api/doctors/me - the signed-in doctor's profile, caseload list, ward statistics and "Add New Everything here is scoped to the caller (a doctor never passes a doctor id), and the whole group is | Owning module or runtime tooling |
| `EndpointExtensions.cs` | Finds every IEndpointGroup in the assembly and maps it. Registration and mapping are two steps because mapping needs the finished service provider. Both are | `src/CardioSense.Api/Endpoints/IEndpointGroup.cs` |
| `HealthEndpoints.cs` | Liveness and readiness probes for containers and load balancers. /health/live only proves the process answers (no checks run). /health/ready additionally runs the | `src/CardioSense.Api/DependencyInjection.cs` |
| `IEndpointGroup.cs` | Contract for a self-registering group of endpoints (one class per area). Program.cs never lists endpoints. Adding an area means adding one class that implements this | `src/CardioSense.Api/Endpoints/AuthEndpoints.cs`, `src/CardioSense.Api/Endpoints/DeviceEndpoints.cs` |
| `MonitoringHubEndpoints.cs` | Maps the SignalR monitoring hub (live ECG, vitals, risk status, alerts) to /hubs/monitoring. The hub class lives in Infrastructure (the realtime notifier needs its type, and Infrastructure | Owning module or runtime tooling |
| `OpenApiEndpoints.cs` | Publishes the machine-readable API description (OpenAPI) at /openapi/v1.json. The document lists every route and body shape, which is the quickest way for the frontend team to | Owning module or runtime tooling |
| `PatientEndpoints.cs` | /api/patients - a patient's own data (/me) and a doctor's view of an assigned patient (/{id}). Both views serve the same shared detail screen, so every read exists twice with the same logic: the | Owning module or runtime tooling |

### src/CardioSense.Api/Http

| File | Responsibility in plain words | Entry point or consumer |
|---|---|---|
| `AllowedOriginFilter.cs` | Blocks cookie-driven auth calls that were triggered from a site that is not the web client. /api/auth/refresh and /logout act on the httpOnly refresh cookie, which a browser attaches | `src/CardioSense.Api/Endpoints/AuthEndpoints.cs` |
| `ApiErrors.cs` | The few errors that only the HTTP layer can produce (everything else comes from Application). Same Error type as the rest of the solution so every failure leaves the API through the one | `src/CardioSense.Api/Http/AllowedOriginFilter.cs`, `src/CardioSense.Api/Middleware/GlobalExceptionHandler.cs` |
| `ResultHttpExtensions.cs` | Turns a Result or an Error from the mediator into the HTTP response. The one mapping from ErrorType to HTTP status (Failure 422, Validation 400, Unauthorized 401, | Owning module or runtime tooling |

### src/CardioSense.Api/Middleware

| File | Responsibility in plain words | Entry point or consumer |
|---|---|---|
| `AuditLoggingMiddleware.cs` | Writes one AuditLogs row for every request that touches a patient's record. Answers "who looked at this patient's file, when and from where" after the fact - a real | `src/CardioSense.Api/Middleware/PatientAuditMetadata.cs`, `src/CardioSense.Api/WebApplicationExtensions.cs` |
| `GlobalExceptionHandler.cs` | Last-resort translation of an unexpected exception into a clean ProblemDetails response. Expected failures are Results and never reach here. What does reach here is either a race the | `src/CardioSense.Api/DependencyInjection.cs` |
| `PatientAuditMetadata.cs` | Marker that tells AuditLoggingMiddleware "this route reads or writes a patient's record". Auditing is opt-in per route group through endpoint metadata, so the middleware needs no list of | `src/CardioSense.Api/Endpoints/PatientEndpoints.cs`, `src/CardioSense.Api/Middleware/AuditLoggingMiddleware.cs` |
| `SecurityHeadersMiddleware.cs` | Adds the response headers every API answer should carry. This API only returns JSON about real patients, so it must never be cached by a browser or a proxy | `src/CardioSense.Api/WebApplicationExtensions.cs` |

### src/CardioSense.Api/Options

| File | Responsibility in plain words | Entry point or consumer |
|---|---|---|
| `ApiCorsOptions.cs` | The browser origins that may call the API with credentials (the React web client). The refresh token travels in an httpOnly cookie, so cross-origin calls must be credentialed and | `src/CardioSense.Api/DependencyInjection.cs`, `src/CardioSense.Api/Http/AllowedOriginFilter.cs` |
| `DatabaseInitialization.cs` | What the host does to the database when it starts. Production applies migrations; local development and tests can build the schema straight from | `src/CardioSense.Api/Options/DatabaseOptions.cs`, `src/CardioSense.Api/WebApplicationExtensions.cs` |
| `DatabaseOptions.cs` | Start-up database settings. See DatabaseInitialization; kept in its own options class so the choice is explicit per | `src/CardioSense.Api/DependencyInjection.cs`, `src/CardioSense.Api/WebApplicationExtensions.cs` |
| `RateLimitOptions.cs` | How many login/signup attempts one client address may make per time window. Password guessing and mass sign-ups are the realistic attacks on a public auth endpoint; a fixed | `src/CardioSense.Api/DependencyInjection.cs` |
| `RefreshCookieOptions.cs` | Settings of the httpOnly cookie that carries the refresh token. The cookie is always HttpOnly and Secure. SameSite is Strict by default (frontend and API on the | `src/CardioSense.Api/Authentication/RefreshCookie.cs`, `src/CardioSense.Api/DependencyInjection.cs` |

### src/CardioSense.Api/Properties

| File | Responsibility in plain words | Entry point or consumer |
|---|---|---|
| `launchSettings.json` | Declares launchSettings.json settings or project metadata for Properties. | Owning module or runtime tooling |

### src/CardioSense.Application/Auth/Commands/Login

| File | Responsibility in plain words | Entry point or consumer |
|---|---|---|
| `LoginCommand.cs` | Request to sign in with e-mail and password. Body of POST /api/auth/login; the result is an AuthSession the Api converts to a JSON body | `src/CardioSense.Api/Endpoints/AuthEndpoints.cs`, `src/CardioSense.Application/Auth/Commands/Login/LoginHandler.cs` |
| `LoginHandler.cs` | Handles a login: verifies the credentials, then issues a session. Unknown e-mail and wrong password produce the same error so the endpoint cannot be used to | Owning module or runtime tooling |
| `LoginValidator.cs` | Validates the login request. Only checks presence/shape (e-mail format, length caps) - password strength is a signup | Owning module or runtime tooling |

### src/CardioSense.Application/Auth/Commands/Logout

| File | Responsibility in plain words | Entry point or consumer |
|---|---|---|
| `LogoutCommand.cs` | Request to end the current session by revoking its refresh token. The Api reads the refresh token from the httpOnly cookie and passes the raw value; a | `src/CardioSense.Api/Endpoints/AuthEndpoints.cs`, `src/CardioSense.Application/Auth/Commands/Logout/LogoutHandler.cs` |
| `LogoutHandler.cs` | Handles a logout by revoking the presented refresh token in the database. Idempotent: no token, unknown token or already-revoked token all succeed, so logging out | Owning module or runtime tooling |

### src/CardioSense.Application/Auth/Commands/RefreshToken

| File | Responsibility in plain words | Entry point or consumer |
|---|---|---|
| `RefreshTokenCommand.cs` | Request to exchange a refresh token for a new access token (and a rotated refresh token). The Api reads the refresh token from the httpOnly cookie; the frontend calls this silently | `src/CardioSense.Api/Endpoints/AuthEndpoints.cs`, `src/CardioSense.Application/Auth/Commands/RefreshToken/RefreshTokenHandler.cs` |
| `RefreshTokenHandler.cs` | Handles token refresh with rotation and reuse detection. A valid token is revoked the moment a new one is issued (rotation). A revoked token | Owning module or runtime tooling |

### src/CardioSense.Application/Auth/Commands/RegisterDoctor

| File | Responsibility in plain words | Entry point or consumer |
|---|---|---|
| `RegisterDoctorCommand.cs` | Request to create a doctor account and profile (the doctor signup form). Body of POST /api/auth/signup/doctor - same fields as DoctorSignupRequest in the frontend. | `src/CardioSense.Api/Endpoints/AuthEndpoints.cs`, `src/CardioSense.Application/Auth/Commands/RegisterDoctor/RegisterDoctorHandler.cs` |
| `RegisterDoctorHandler.cs` | Handles doctor signup: account + profile in one transaction, then a session. A license number identifies one doctor, so a second signup with the same license is | Owning module or runtime tooling |
| `RegisterDoctorValidator.cs` | Validates the doctor signup request. Same rules and length limits as doctorSignupSchema (Zod) in the frontend; limits come from | Owning module or runtime tooling |

### src/CardioSense.Application/Auth/Commands/RegisterPatient

| File | Responsibility in plain words | Entry point or consumer |
|---|---|---|
| `RegisterPatientCommand.cs` | Request to create a patient account and profile (the patient signup form). Body of POST /api/auth/signup/patient - same fields as PatientSignupRequest in the | `src/CardioSense.Api/Endpoints/AuthEndpoints.cs`, `src/CardioSense.Application/Auth/Commands/RegisterPatient/RegisterPatientHandler.cs` |
| `RegisterPatientHandler.cs` | Handles patient signup: account + profile + medical history, optional doctor link and Account and profile are created in ONE transaction so no orphan login can exist. The | Owning module or runtime tooling |
| `RegisterPatientValidator.cs` | Validates the patient signup request. Same rules and length limits as patientSignupSchema (Zod) in the frontend, so client and | Owning module or runtime tooling |

### src/CardioSense.Application/Auth/Common

| File | Responsibility in plain words | Entry point or consumer |
|---|---|---|
| `AuthDtos.cs` | Auth response contracts: the signed-in user, the login/signup response and the refresh These are the exact bodies the frontend expects (AuthenticatedUser, AuthResponse, | Owning module or runtime tooling |
| `AuthSession.cs` | Internal result of any sign-in: access token, raw refresh token (for the cookie) and the The Api layer splits it: the raw refresh token and its expiry become the httpOnly cookie, | `src/CardioSense.Api/Endpoints/AuthEndpoints.cs`, `src/CardioSense.Application/Auth/Commands/Login/LoginCommand.cs` |
| `SessionIssuer.cs` | Issues a session for an account: finds the profile, creates the access token, creates and Login, both signups and token refresh all end the same way, so the steps live here once | `src/CardioSense.Application/Auth/Commands/Login/LoginHandler.cs`, `src/CardioSense.Application/Auth/Commands/RefreshToken/RefreshTokenHandler.cs` |

### src/CardioSense.Application

| File | Responsibility in plain words | Entry point or consumer |
|---|---|---|
| `CardioSense.Application.csproj` | Declares CardioSense.Application.csproj settings or project metadata for CardioSense.Application. | Owning module or runtime tooling |
| `DependencyInjection.cs` | Single entry point that registers everything the Application layer needs (Mediator, Called once from the host: services.AddApplication(). The Mediator source generator lives | `Directory.Packages.props`, `src/CardioSense.Api/DependencyInjection.cs` |

### src/CardioSense.Application/Common/Behaviors

| File | Responsibility in plain words | Entry point or consumer |
|---|---|---|
| `AuthorizationBehavior.cs` | Mediator pipeline behavior that enforces patient-level access for every patient-scoped If the request implements IPatientScopedRequest and the caller may not access that | `src/CardioSense.Application/Common/Interfaces/IPatientAccessChecker.cs`, `src/CardioSense.Application/Common/Interfaces/IPatientScopedRequest.cs` |
| `ResultFailureFactory.cs` | Builds a failed Result / Result<T> of a given response type from an Error. Pipeline behaviors are generic over the response type but must short-circuit with a | `src/CardioSense.Application/Common/Behaviors/AuthorizationBehavior.cs`, `src/CardioSense.Application/Common/Behaviors/ValidationBehavior.cs` |
| `ValidationBehavior.cs` | Mediator pipeline behavior that runs every FluentValidation validator before the handler. Invalid input never reaches a handler: the pipeline returns a failed Result of type | `src/CardioSense.Application/DependencyInjection.cs` |

### src/CardioSense.Application/Common/Contracts

| File | Responsibility in plain words | Entry point or consumer |
|---|---|---|
| `ContractLiterals.cs` | The literal string values of the frontend contract (languages, genders, history ranges, The frontend sends/expects lower-case literals such as 'ar', 'male', '24h', 'daily', | `src/CardioSense.Api/Endpoints/DoctorEndpoints.cs`, `src/CardioSense.Api/Endpoints/PatientEndpoints.cs` |

### src/CardioSense.Application/Common/Dtos

| File | Responsibility in plain words | Entry point or consumer |
|---|---|---|
| `AiReportDto.cs` | AI clinical report for a period: narrative text plus the numbers behind it. The narrative comes from the AI service and is displayed verbatim; the numbers are | `src/CardioSense.Application/Patients/Queries/GetPatientAiReport/GetPatientAiReportHandler.cs`, `src/CardioSense.Application/Patients/Queries/GetPatientAiReport/GetPatientAiReportQuery.cs` |
| `AlertDto.cs` | A clinical alert as shown in the alerts timeline (also the payload of the Type is the human-readable text the UI renders verbatim (for example 'Manual Event'). | `src/CardioSense.Application/Common/Interfaces/IRealtimeNotifier.cs`, `src/CardioSense.Application/Common/Mappings/AlertMappings.cs` |
| `ClinicalDtos.cs` | Latest-reading DTOs: vitals sample, ECG feature set, AI risk assessment, electrode Doctor-only fields of the risk assessment (confidence, score, flags) are nullable/empty so | Owning module or runtime tooling |
| `DeviceSummaryDto.cs` | Device card data: serial, firmware, battery, last seen, transport and signal. Serves both the profile aggregate and the device endpoints (the frontend's | `src/CardioSense.Application/Common/Dtos/PatientProfileDto.cs`, `src/CardioSense.Application/Common/Mappings/DeviceMappings.cs` |
| `DoctorProfileDto.cs` | Doctor profile returned by GET/PUT /doctors/me. Combines the Doctors row with the account's e-mail and preferred language. | `src/CardioSense.Application/Common/Mappings/ProfileMappings.cs`, `src/CardioSense.Application/Doctors/Commands/UpdateDoctorProfile/UpdateDoctorProfileCommand.cs` |
| `PagedResult.cs` | Generic page of items (PaginatedResponse<T> in the frontend contract). TotalCount is the number of items after filtering and before paging; TotalPages is derived | `src/CardioSense.Application/Doctors/Queries/GetMyPatients/GetMyPatientsHandler.cs`, `src/CardioSense.Application/Doctors/Queries/GetMyPatients/GetMyPatientsQuery.cs` |
| `PatientListItemDto.cs` | One row of the doctor's patient list. Bed/room, blood pressure and ST elevation of the TypeScript type are optional there and | `src/CardioSense.Application/Doctors/Common/PatientListItemAssembler.cs`, `src/CardioSense.Application/Doctors/Queries/GetMyPatients/GetMyPatientsHandler.cs` |
| `PatientProfileDto.cs` | The full patient aggregate returned by GET /patients/me and GET /patients/{id}. One call returns profile + latest vitals + latest AI assessment + latest electrode check + | `src/CardioSense.Application/Patients/Commands/UpdatePatientProfile/UpdatePatientProfileCommand.cs`, `src/CardioSense.Application/Patients/Commands/UpdatePatientProfile/UpdatePatientProfileHandler.cs` |
| `RealtimePayloads.cs` | Payloads pushed over SignalR: vitals tick, risk status (clinical + patient-safe variants), RiskStatusBroadcast carries both variants so the Application layer - which owns the | Owning module or runtime tooling |
| `WardStatsDto.cs` | Ward-level numbers of the doctor dashboard (active patients, critical cases, devices, Counts cover all of the doctor's assigned patients regardless of the list's | `src/CardioSense.Application/Doctors/Queries/GetWardStats/GetWardStatsHandler.cs`, `src/CardioSense.Application/Doctors/Queries/GetWardStats/GetWardStatsQuery.cs` |

### src/CardioSense.Application/Common/Errors

| File | Responsibility in plain words | Entry point or consumer |
|---|---|---|
| `ApplicationErrors.cs` | Catalogue of the errors raised by use cases (handlers, behaviors) - stable codes plus Domain rules own DomainErrors; everything that depends on data lookups or the caller (not | `src/CardioSense.Api/Authorization/PatientAccessFilter.cs`, `src/CardioSense.Api/Endpoints/PatientEndpoints.cs` |

### src/CardioSense.Application/Common/Interfaces

| File | Responsibility in plain words | Entry point or consumer |
|---|---|---|
| `IAppDbContext.cs` | Abstraction of the relational database used by handlers (one DbSet per aggregate + save + Handlers query EF Core directly through this interface instead of one repository per | `src/CardioSense.Api/Middleware/AuditLoggingMiddleware.cs`, `src/CardioSense.Application/Auth/Commands/Logout/LogoutHandler.cs` |
| `ICurrentUser.cs` | Who is making the current request (account, role and profile ids). Handlers never touch HttpContext; they ask this interface. Profile ids come from JWT | `src/CardioSense.Api/Authentication/CurrentUser.cs`, `src/CardioSense.Api/Authentication/CurrentUserHubFilter.cs` |
| `IIdentityService.cs` | Account management contract (create account, verify credentials, read account, change Hides ASP.NET Core Identity: passwords, lockout and the Users table stay in | `src/CardioSense.Application/Auth/Commands/Login/LoginHandler.cs`, `src/CardioSense.Application/Auth/Commands/RefreshToken/RefreshTokenHandler.cs` |
| `IPatientAccessChecker.cs` | The single place that decides 'may the current caller access this patient?'. A patient may access only themself; a doctor only patients assigned to them. Used by | `src/CardioSense.Api/Authentication/CurrentUserHubFilter.cs`, `src/CardioSense.Api/Authorization/PatientAccessHandler.cs` |
| `IPatientScopedRequest.cs` | Marker for requests that act on one specific patient. AuthorizationBehavior inspects this marker and refuses the request unless the caller may | `src/CardioSense.Application/Common/Behaviors/AuthorizationBehavior.cs`, `src/CardioSense.Application/Patients/Commands/FreezeWaveform/FreezeWaveformCommand.cs` |
| `IRealtimeNotifier.cs` | Push channel to browsers (SignalR): live vitals, ECG samples, risk status, new alerts, Event names the frontend listens to: vitals:{patientId}, ecg:{patientId}, | `src/CardioSense.Application/Patients/Commands/LogManualEvent/LogManualEventHandler.cs`, `src/CardioSense.Application/Telemetry/Commands/ProcessTelemetry/ProcessTelemetryHandler.cs` |
| `IRiskAssessmentService.cs` | Gateway to the AI service: risk assessment of a reading window and the report narrative Failures (service down, timeout, invalid answer) come back as Error.Unavailable so a | `src/CardioSense.Application/Patients/Queries/GetPatientAiReport/GetPatientAiReportHandler.cs`, `src/CardioSense.Application/Telemetry/Commands/ProcessTelemetry/ProcessTelemetryHandler.cs` |
| `ITokenService.cs` | Token contract: create JWT access tokens, generate refresh tokens and hash them for Access tokens are short-lived and carry sub/role/patientId/doctorId; refresh tokens are | `src/CardioSense.Application/Auth/Commands/Logout/LogoutHandler.cs`, `src/CardioSense.Application/Auth/Commands/RefreshToken/RefreshTokenHandler.cs` |

### src/CardioSense.Application/Common/Mappings

| File | Responsibility in plain words | Entry point or consumer |
|---|---|---|
| `AlertMappings.cs` | Maps an Alert entity to AlertDto and turns AlertType into the text the UI shows. The frontend renders Alert.type verbatim, so the enum is converted to a readable label | Owning module or runtime tooling |
| `ClinicalMappings.cs` | Maps vitals, ECG features, risk assessments and electrode checks to their DTOs. The risk assessment mapping takes includeClinicalDetails: patients get status only - | Owning module or runtime tooling |
| `DeviceMappings.cs` | Maps a Device entity to DeviceSummaryDto. A device that has never reported falls back to the pairing (then creation) time for | Owning module or runtime tooling |
| `ProfileMappings.cs` | Maps profile-related entities: history entry, doctor (as assigned doctor and as profile) The doctor profile needs the account's e-mail/language, so that mapping receives the | Owning module or runtime tooling |
| `StatusLabels.cs` | The label shown on a status pill (Fully Stable / Clinical Alert / Critical Alert / A patient with no assessment yet is labelled 'Awaiting Data' instead of silently looking | `src/CardioSense.Application/Doctors/Common/PatientListItemAssembler.cs`, `src/CardioSense.Application/Telemetry/Commands/ProcessTelemetry/ProcessTelemetryHandler.cs` |

### src/CardioSense.Application/Common/Models

| File | Responsibility in plain words | Entry point or consumer |
|---|---|---|
| `AiModels.cs` | Request/response shapes of the AI service gateway (risk assessment and report narrative). They mirror the documented POST /analyze contract (features + patient context in, status + | Owning module or runtime tooling |
| `IdentityModels.cs` | Data shapes exchanged with IIdentityService (new account request, account summary). Keeps ASP.NET Identity types out of the Application layer - only plain records cross the | Owning module or runtime tooling |
| `TokenModels.cs` | Data shapes exchanged with ITokenService (what goes into a JWT, the issued JWT, a fresh The raw refresh token value is returned once (to be set as an httpOnly cookie); only its | Owning module or runtime tooling |

### src/CardioSense.Application/Common/Options

| File | Responsibility in plain words | Entry point or consumer |
|---|---|---|
| `MonitoringOptions.cs` | Tunable monitoring settings (doctor caseload capacity, how recent a device message must be Defaults work out of the box; the Api layer may override them from the 'Monitoring' | `src/CardioSense.Application/DependencyInjection.cs`, `src/CardioSense.Application/Doctors/Common/PatientListItemAssembler.cs` |

### src/CardioSense.Application/Common/Security

| File | Responsibility in plain words | Entry point or consumer |
|---|---|---|
| `PatientAccessChecker.cs` | Implements the access rule: a patient sees only themself, a doctor only patients assigned Patients are decided from claims alone (no database hit); doctors need one existence query | `src/CardioSense.Application/Common/Interfaces/IPatientAccessChecker.cs`, `src/CardioSense.Application/DependencyInjection.cs` |

### src/CardioSense.Application/Common/Validation

| File | Responsibility in plain words | Entry point or consumer |
|---|---|---|
| `CommonRules.cs` | Reusable FluentValidation rules shared by several validators (email, password, national Mirrors the frontend Zod schemas exactly (email format, password min 8, national id = 14 | `src/CardioSense.Application/Auth/Commands/Login/LoginValidator.cs` |

### src/CardioSense.Application/Doctors/Commands/UpdateDoctorProfile

| File | Responsibility in plain words | Entry point or consumer |
|---|---|---|
| `UpdateDoctorProfileCommand.cs` | Request for a doctor to update their own profile (partial update). Body of PUT /api/doctors/me: only the fields that are present change; the license number | `src/CardioSense.Api/Endpoints/DoctorEndpoints.cs`, `src/CardioSense.Application/Doctors/Commands/UpdateDoctorProfile/UpdateDoctorProfileHandler.cs` |
| `UpdateDoctorProfileHandler.cs` | Handles a doctor's own profile update and returns the refreshed profile. Professional fields are saved first, then the language preference on the account, then the | Owning module or runtime tooling |
| `UpdateDoctorProfileValidator.cs` | Validates the partial doctor profile update. A field is validated only when it is present; present values follow the same limits as | Owning module or runtime tooling |

### src/CardioSense.Application/Doctors/Common

| File | Responsibility in plain words | Entry point or consumer |
|---|---|---|
| `DoctorCaseload.cs` | Shared queries about a doctor's caseload: assigned patient ids, newest assessment and Used by the patient list and the ward stats so 'latest per patient' is computed the same | Owning module or runtime tooling |
| `PatientListItemAssembler.cs` | Builds the list rows (PatientListItemDto) for a set of patients. Used by GetMyPatients and by AssignDoctor (which returns the newly added row). A patient | `src/CardioSense.Application/DependencyInjection.cs`, `src/CardioSense.Application/Doctors/Queries/GetMyPatients/GetMyPatientsHandler.cs` |

### src/CardioSense.Application/Doctors/Queries/GetDoctorProfile

| File | Responsibility in plain words | Entry point or consumer |
|---|---|---|
| `GetDoctorProfileHandler.cs` | Handles GetDoctorProfileQuery. The doctor is whoever is authenticated; the account's e-mail and language come from the | Owning module or runtime tooling |
| `GetDoctorProfileQuery.cs` | Query for the signed-in doctor's own profile. Serves GET /api/doctors/me, which the frontend calls right after a silent refresh to learn | `src/CardioSense.Api/Endpoints/DoctorEndpoints.cs`, `src/CardioSense.Application/Doctors/Queries/GetDoctorProfile/GetDoctorProfileHandler.cs` |

### src/CardioSense.Application/Doctors/Queries/GetMyPatients

| File | Responsibility in plain words | Entry point or consumer |
|---|---|---|
| `GetMyPatientsHandler.cs` | Handles the doctor's patient list. Takes only the doctor's own caseload, filters by status and search text, orders the | Owning module or runtime tooling |
| `GetMyPatientsQuery.cs` | Query for the doctor's patient list (status filter, search, paging). Serves GET /api/doctors/me/patients?status=&search=&page=. Defaults match what the | `src/CardioSense.Api/Endpoints/DoctorEndpoints.cs`, `src/CardioSense.Application/Doctors/Queries/GetMyPatients/GetMyPatientsHandler.cs` |
| `GetMyPatientsValidator.cs` | Validates the patient list query parameters. Status must be a known filter, the page is 1-based and the page size is capped. | Owning module or runtime tooling |

### src/CardioSense.Application/Doctors/Queries/GetWardStats

| File | Responsibility in plain words | Entry point or consumer |
|---|---|---|
| `GetWardStatsHandler.cs` | Handles the ward statistics of one doctor. '42 active patients' means patients assigned to this doctor. Status counts add up to the | Owning module or runtime tooling |
| `GetWardStatsQuery.cs` | Query for the doctor's ward statistics cards. Serves GET /api/doctors/me/ward-stats; it has no parameters because the doctor is the | `src/CardioSense.Api/Endpoints/DoctorEndpoints.cs`, `src/CardioSense.Application/Doctors/Queries/GetWardStats/GetWardStatsHandler.cs` |

### src/CardioSense.Application/Patients/Commands/AssignDoctor

| File | Responsibility in plain words | Entry point or consumer |
|---|---|---|
| `AssignDoctorCommand.cs` | Request for the signed-in doctor to add a patient to their caseload by national id. Body of POST /api/doctors/me/patients ({identifier}) - the 'Add New Patient' button. | `src/CardioSense.Api/Endpoints/DoctorEndpoints.cs`, `src/CardioSense.Application/Patients/Commands/AssignDoctor/AssignDoctorHandler.cs` |
| `AssignDoctorHandler.cs` | Handles adding a patient to the doctor's caseload. Looks the patient up by national id, creates the doctor-patient assignment (primary when | Owning module or runtime tooling |
| `AssignDoctorValidator.cs` | Validates the add-patient request. The identifier is required and capped so a huge value cannot reach the database. | Owning module or runtime tooling |

### src/CardioSense.Application/Patients/Commands/FreezeWaveform

| File | Responsibility in plain words | Entry point or consumer |
|---|---|---|
| `FreezeWaveformCommand.cs` | Request for a doctor to freeze or unfreeze the live waveform of a patient. Body of POST /api/patients/{id}/freeze-waveform ({frozen}). Freezing is a presentation | `src/CardioSense.Api/Endpoints/PatientEndpoints.cs`, `src/CardioSense.Application/Patients/Commands/FreezeWaveform/FreezeWaveformHandler.cs` |
| `FreezeWaveformHandler.cs` | Handles the freeze/unfreeze acknowledgement. Raw waveform is never stored, so there is no database change: the handler only allows | Owning module or runtime tooling |

### src/CardioSense.Application/Patients/Commands/LogManualEvent

| File | Responsibility in plain words | Entry point or consumer |
|---|---|---|
| `LogManualEventCommand.cs` | Request for a doctor to log a manual clinical event on a patient. Body of POST /api/patients/{id}/manual-events (note + severity). The event becomes an | `src/CardioSense.Api/Endpoints/PatientEndpoints.cs`, `src/CardioSense.Application/Patients/Commands/LogManualEvent/LogManualEventHandler.cs` |
| `LogManualEventHandler.cs` | Handles a manual event: creates the alert, saves it and pushes it to everyone watching the Doctor-only action. A Critical manual event also latches the patient's status until a | Owning module or runtime tooling |
| `LogManualEventValidator.cs` | Validates the manual event request. A note is required (it becomes the alert message) and the severity must be a defined | Owning module or runtime tooling |

### src/CardioSense.Application/Patients/Commands/PairDevice

| File | Responsibility in plain words | Entry point or consumer |
|---|---|---|
| `PairDeviceCommand.cs` | Request to pair a device (by serial number) to a patient. Body of POST /api/devices/pair. A patient omits PatientId (it means 'me'); a doctor must | `src/CardioSense.Api/Endpoints/DeviceEndpoints.cs`, `src/CardioSense.Application/Patients/Commands/PairDevice/PairDeviceHandler.cs` |
| `PairDeviceHandler.cs` | Handles device pairing: resolves the target patient, checks access, then pairs an Only devices that already exist in the inventory can be paired (unknown serials are 'not | Owning module or runtime tooling |
| `PairDeviceValidator.cs` | Validates the pair-device request. A serial number is required and capped at the length the Device entity allows. | Owning module or runtime tooling |

### src/CardioSense.Application/Patients/Commands/ResolveAlert

| File | Responsibility in plain words | Entry point or consumer |
|---|---|---|
| `ResolveAlertCommand.cs` | Request for a doctor to resolve an alert (with an optional note). Added so the documented domain rules are reachable: Alert.Resolve (critical alerts need a | `src/CardioSense.Api/Endpoints/PatientEndpoints.cs`, `src/CardioSense.Application/Patients/Commands/ResolveAlert/ResolveAlertHandler.cs` |
| `ResolveAlertHandler.cs` | Handles alert resolution by a doctor. The alert must belong to the patient in the request (so an id from another patient cannot | Owning module or runtime tooling |
| `ResolveAlertValidator.cs` | Validates the resolve-alert request. The alert id is required and the optional note respects the same length limit as the alert | Owning module or runtime tooling |

### src/CardioSense.Application/Patients/Commands/UpdatePatientProfile

| File | Responsibility in plain words | Entry point or consumer |
|---|---|---|
| `UpdatePatientProfileCommand.cs` | Request for a patient to update their own profile (partial update). Body of PUT /api/patients/me: only the fields that are present change. Medical history is | `src/CardioSense.Api/Endpoints/PatientEndpoints.cs`, `src/CardioSense.Application/Patients/Commands/UpdatePatientProfile/UpdatePatientProfileHandler.cs` |
| `UpdatePatientProfileHandler.cs` | Handles a patient's own profile update and returns the refreshed aggregate. Only the patient may edit their profile (an assigned doctor passes the access check but | Owning module or runtime tooling |
| `UpdatePatientProfileValidator.cs` | Validates the partial profile update. A field is validated only when it is present; present values follow the same limits as | Owning module or runtime tooling |

### src/CardioSense.Application/Patients/Common

| File | Responsibility in plain words | Entry point or consumer |
|---|---|---|
| `PatientProfileAssembler.cs` | Builds the full PatientProfileDto aggregate (profile + latest clinical snapshot + 24h Used by GetPatientProfile and UpdatePatientProfile so the aggregate is assembled in | `src/CardioSense.Application/DependencyInjection.cs`, `src/CardioSense.Application/Patients/Commands/UpdatePatientProfile/UpdatePatientProfileHandler.cs` |

### src/CardioSense.Application/Patients/Queries/GetDeviceStatus

| File | Responsibility in plain words | Entry point or consumer |
|---|---|---|
| `GetDeviceStatusHandler.cs` | Handles GetDeviceStatusQuery. A device the caller may not see answers 'not found' exactly like a missing one, so serial | Owning module or runtime tooling |
| `GetDeviceStatusQuery.cs` | Query for a device's status card data by serial number. Serves GET /devices/{serial}/status. Not patient-scoped by id (the route has a serial), so | `src/CardioSense.Api/Endpoints/DeviceEndpoints.cs`, `src/CardioSense.Application/Patients/Queries/GetDeviceStatus/GetDeviceStatusHandler.cs` |
| `GetDeviceStatusValidator.cs` | Validates the serial number of the device status query. A serial is required and capped at the length the Device entity allows. | Owning module or runtime tooling |

### src/CardioSense.Application/Patients/Queries/GetPatientAiReport

| File | Responsibility in plain words | Entry point or consumer |
|---|---|---|
| `GetPatientAiReportHandler.cs` | Handles GetPatientAiReportQuery: computes the period's numbers, then asks the AI service No data in the period means NotFound (the UI then shows its empty state) - a report is | Owning module or runtime tooling |
| `GetPatientAiReportQuery.cs` | Query for the AI clinical report of a patient for a period (daily, weekly or monthly). Serves GET /patients/{id}/ai-report?period= and the /me variant (an endpoint the | `src/CardioSense.Api/Endpoints/PatientEndpoints.cs`, `src/CardioSense.Application/Patients/Queries/GetPatientAiReport/GetPatientAiReportHandler.cs` |
| `GetPatientAiReportValidator.cs` | Validates the period of the AI report query. Only daily, weekly and monthly are accepted (the same set the frontend sends). | Owning module or runtime tooling |

### src/CardioSense.Application/Patients/Queries/GetPatientAlerts

| File | Responsibility in plain words | Entry point or consumer |
|---|---|---|
| `GetPatientAlertsHandler.cs` | Handles GetPatientAlertsQuery. Returns the newest alerts first and caps the list so an old, noisy patient file cannot | Owning module or runtime tooling |
| `GetPatientAlertsQuery.cs` | Query for a patient's alert timeline (newest first). Serves GET /patients/{id}/alerts and the /me variant. | `src/CardioSense.Api/Endpoints/PatientEndpoints.cs`, `src/CardioSense.Application/Patients/Queries/GetPatientAlerts/GetPatientAlertsHandler.cs` |

### src/CardioSense.Application/Patients/Queries/GetPatientProfile

| File | Responsibility in plain words | Entry point or consumer |
|---|---|---|
| `GetPatientProfileHandler.cs` | Handles GetPatientProfileQuery. Doctors receive the clinical risk details; patients receive the status only (server-side | Owning module or runtime tooling |
| `GetPatientProfileQuery.cs` | Query for one patient's full profile aggregate. Serves GET /patients/me (the Api passes the caller's own patient id) and GET | `src/CardioSense.Api/Endpoints/PatientEndpoints.cs`, `src/CardioSense.Application/Patients/Queries/GetPatientProfile/GetPatientProfileHandler.cs` |

### src/CardioSense.Application/Patients/Queries/GetPatientVitalsHistory

| File | Responsibility in plain words | Entry point or consumer |
|---|---|---|
| `GetPatientVitalsHistoryHandler.cs` | Handles GetPatientVitalsHistoryQuery. Each range maps to a window and a bucket size so the chart always receives at most one | Owning module or runtime tooling |
| `GetPatientVitalsHistoryQuery.cs` | Query for a patient's down-sampled vitals history for a range (24h, 7d or 30d). Serves GET /patients/{id}/vitals-history?range= and the /me variant; the range literal is | `src/CardioSense.Api/Endpoints/PatientEndpoints.cs`, `src/CardioSense.Application/Patients/Queries/GetPatientVitalsHistory/GetPatientVitalsHistoryHandler.cs` |
| `GetPatientVitalsHistoryValidator.cs` | Validates the range of the vitals history query. Only the literals 24h, 7d and 30d are accepted (the same set the frontend sends). | Owning module or runtime tooling |

### src/CardioSense.Application/Telemetry/Commands/ProcessDeviceStatus

| File | Responsibility in plain words | Entry point or consumer |
|---|---|---|
| `ProcessDeviceStatusCommand.cs` | One status message from a device: battery, transport, firmware and signal strength. Sent by the MQTT ingestion worker for messages on devices/{serial}/status (the slower | `src/CardioSense.Application/Telemetry/Commands/ProcessDeviceStatus/ProcessDeviceStatusHandler.cs`, `src/CardioSense.Application/Telemetry/Commands/ProcessDeviceStatus/ProcessDeviceStatusValidator.cs` |
| `ProcessDeviceStatusHandler.cs` | Handles a device status message by updating the device row. Works for any inventory device (paired or not) because a freshly shipped device may report | Owning module or runtime tooling |
| `ProcessDeviceStatusValidator.cs` | Validates a device status message. Battery is a percentage, the transport must be a defined value and the signal strength a | Owning module or runtime tooling |

### src/CardioSense.Application/Telemetry/Commands/ProcessTelemetry

| File | Responsibility in plain words | Entry point or consumer |
|---|---|---|
| `ProcessTelemetryCommand.cs` | One telemetry message from a device: vitals, ECG features and (optionally) a window of Sent by the MQTT ingestion worker for every message on devices/{serial}/telemetry. The | `src/CardioSense.Application/Telemetry/Commands/ProcessTelemetry/ProcessTelemetryHandler.cs`, `src/CardioSense.Application/Telemetry/Commands/ProcessTelemetry/ProcessTelemetryValidator.cs` |
| `ProcessTelemetryHandler.cs` | The core use case: device message in -> stored vitals/features -> live push -> AI Order matters: what the device measured is saved and shown FIRST, so an AI outage never | `src/CardioSense.Infrastructure/Ingestion/MqttIngestionService.cs` |
| `ProcessTelemetryValidator.cs` | Validates a telemetry message before it is processed. Bounds come from the entities (single source of truth). A timestamp more than five minutes | Owning module or runtime tooling |

### src/CardioSense.Domain

| File | Responsibility in plain words | Entry point or consumer |
|---|---|---|
| `CardioSense.Domain.csproj` | Declares CardioSense.Domain.csproj settings or project metadata for CardioSense.Domain. | Owning module or runtime tooling |

### src/CardioSense.Domain/Common

| File | Responsibility in plain words | Entry point or consumer |
|---|---|---|
| `DomainErrors.cs` | Catalogue of the errors that entity behaviour can return (stable codes + messages). One nested class per entity so call sites read DomainErrors.Alert.CriticalRequiresDoctor; | `src/CardioSense.Application/Common/Errors/ApplicationErrors.cs`, `src/CardioSense.Domain/Entities/Alert.cs` |
| `Entity.cs` | Base class of every entity: identity (Id) and creation time (CreatedAt). Ids are time-ordered GUIDs (v7) generated by the entity itself, so no database round-trip | `src/CardioSense.Domain/Entities/Alert.cs`, `src/CardioSense.Domain/Entities/AuditLogEntry.cs` |
| `Error.cs` | Immutable description of why an operation failed (code + message + category). Expected failures travel as values (Result) instead of exceptions; the category lets the | `src/CardioSense.Api/Endpoints/AuthEndpoints.cs`, `src/CardioSense.Api/Http/ApiErrors.cs` |
| `ErrorType.cs` | Classifies a failed Result so outer layers can pick the right HTTP status. Failure=422, Validation=400, Unauthorized=401, Forbidden=403, NotFound=404, Conflict=409, | `src/CardioSense.Api/Endpoints/AuthEndpoints.cs`, `src/CardioSense.Api/Http/ResultHttpExtensions.cs` |
| `Result.cs` | The Result pattern - every use case and domain rule returns success or an Error. A Result is either successful (no error) or failed (exactly one Error); Result<T> adds a | `src/CardioSense.Api/CardioSense.Api.csproj`, `src/CardioSense.Api/Endpoints/AuthEndpoints.cs` |

### src/CardioSense.Domain/Entities

| File | Responsibility in plain words | Entry point or consumer |
|---|---|---|
| `Alert.cs` | A clinical alert raised by the system (threshold crossed) or logged manually by a doctor. Critical alerts can only be resolved by a doctor and an alert can be resolved only once - | `src/CardioSense.Application/Common/Contracts/ContractLiterals.cs`, `src/CardioSense.Application/Common/Errors/ApplicationErrors.cs` |
| `AuditLogEntry.cs` | One audit row: who touched which record, when and from where. Written for every read/write of a patient record so 'who looked at this file' can be | `src/CardioSense.Api/Middleware/AuditLoggingMiddleware.cs`, `src/CardioSense.Application/Common/Interfaces/IAppDbContext.cs` |
| `Device.cs` | A wearable device (ESP32 + sensors) identified by its serial number. A device exists in inventory first and is paired to at most one patient (rule from the | `src/CardioSense.Api/Endpoints/DeviceEndpoints.cs`, `src/CardioSense.Application/Auth/Commands/RegisterPatient/RegisterPatientValidator.cs` |
| `Doctor.cs` | Professional profile of a treating doctor. License number and account link are fixed after creation; other professional fields can be | `src/CardioSense.Api/Authorization/AuthorizationPolicies.cs`, `src/CardioSense.Api/Endpoints/DoctorEndpoints.cs` |
| `DoctorPatientAssignment.cs` | Link between a doctor and a patient they supervise. Authorization checks this table on every doctor-side patient request; a patient has at | `src/CardioSense.Application/Auth/Commands/RegisterPatient/RegisterPatientHandler.cs`, `src/CardioSense.Application/Common/Interfaces/IAppDbContext.cs` |
| `EcgFeatureSet.cs` | Processed ECG features extracted on the ESP32 (PR, QRS, QT, QTc) - not the raw waveform. Intervals must be positive durations; the raw feature vector is kept as JSON for future | `src/CardioSense.Application/Common/Interfaces/IAppDbContext.cs`, `src/CardioSense.Application/Common/Mappings/ClinicalMappings.cs` |
| `ElectrodeVerification.cs` | Result of the camera-based electrode placement check. Placement quality is reported as a status plus a 0..1 confidence; the UI shows confidence | `src/CardioSense.Application/Common/Interfaces/IAppDbContext.cs`, `src/CardioSense.Application/Common/Mappings/ClinicalMappings.cs` |
| `MedicalHistoryEntry.cs` | One row of a patient's medical history (diagnosis, medication or condition). Kept as discrete rows (not one text blob) so the UI can list and edit entries individually | `src/CardioSense.Application/Auth/Commands/RegisterPatient/RegisterPatientHandler.cs`, `src/CardioSense.Application/Auth/Commands/RegisterPatient/RegisterPatientValidator.cs` |
| `Patient.cs` | Demographic and identity half of a patient's profile. Age is derived from the date of birth; contact data can be updated, identity data cannot. | `src/CardioSense.Api/Authorization/AuthorizationPolicies.cs`, `src/CardioSense.Api/Authorization/PatientAccessFilter.cs` |
| `RefreshToken.cs` | Server-side record of an issued refresh token (stored hashed, never in plaintext). Tokens are rotated on every use: the old one is revoked the moment a new one is issued, so | `src/CardioSense.Api/Endpoints/AuthEndpoints.cs`, `src/CardioSense.Application/Auth/Commands/RefreshToken/RefreshTokenCommand.cs` |
| `RiskAssessment.cs` | One AI evaluation of a patient's cardiac risk (Stable / Caution / Critical + confidence + Safety rule: while a critical alert is open the stored status can never drop below | `src/CardioSense.Application/Common/Interfaces/IAppDbContext.cs`, `src/CardioSense.Application/Common/Mappings/ClinicalMappings.cs` |
| `VitalsSample.cs` | One heart-rate / SpO2 reading from a device (time-series row). Readings outside the sensor's plausible window are rejected as artefacts instead of being | `src/CardioSense.Application/Common/Mappings/ClinicalMappings.cs`, `src/CardioSense.Application/Telemetry/Commands/ProcessTelemetry/ProcessTelemetryHandler.cs` |

### src/CardioSense.Domain/Enums

| File | Responsibility in plain words | Entry point or consumer |
|---|---|---|
| `AlertSeverity.cs` | Severity of an alert (matches the badges shown in the dashboards). Ordered by severity; only Critical alerts have the doctor-only resolution rule. | `src/CardioSense.Api/Contracts/PatientRequests.cs`, `src/CardioSense.Application/Common/Dtos/AlertDto.cs` |
| `AlertType.cs` | What raised an alert. The display text shown in the UI (for example Risk Threshold) is produced by the | `src/CardioSense.Application/Common/Mappings/AlertMappings.cs`, `src/CardioSense.Application/Patients/Commands/LogManualEvent/LogManualEventHandler.cs` |
| `ConnectionMode.cs` | How a device reaches the backend. WiFi publishes straight to the MQTT broker; Bluetooth goes through the mobile app gateway. | `src/CardioSense.Application/Common/Dtos/DeviceSummaryDto.cs`, `src/CardioSense.Application/Common/Mappings/DeviceMappings.cs` |
| `ElectrodePlacementStatus.cs` | Result of the camera-based electrode placement check. Optimal means readings can be trusted; RepositionNeeded means the electrodes should be | `src/CardioSense.Application/Common/Mappings/ClinicalMappings.cs`, `src/CardioSense.Domain/Entities/ElectrodeVerification.cs` |
| `Gender.cs` | Patient gender as collected at signup. Mapped to the lower-case strings male/female used by the frontend contract. | `src/CardioSense.Application/Auth/Commands/RegisterPatient/RegisterPatientCommand.cs`, `src/CardioSense.Application/Auth/Commands/RegisterPatient/RegisterPatientHandler.cs` |
| `MedicalHistoryEntryType.cs` | Kind of a medical history row. Diagnosis and Condition describe the patient's history; Medication lists current | `src/CardioSense.Application/Auth/Commands/RegisterPatient/RegisterPatientHandler.cs`, `src/CardioSense.Application/Common/Dtos/PatientProfileDto.cs` |
| `RiskStatusLevel.cs` | Patient status shown in the UI (Stable, Caution, Critical). Ordered by severity (higher value = worse) so Max() over a set of levels yields the worst | `src/CardioSense.Application/Common/Contracts/ContractLiterals.cs`, `src/CardioSense.Application/Common/Dtos/ClinicalDtos.cs` |
| `UserRole.cs` | Role of an authenticated account. Drives authorization and decides which profile table (Patients or Doctors) belongs to the | `src/CardioSense.Api/Authentication/CurrentUser.cs`, `src/CardioSense.Api/Authorization/AuthorizationPolicies.cs` |

### src/CardioSense.Domain/Exceptions

| File | Responsibility in plain words | Entry point or consumer |
|---|---|---|
| `DomainException.cs` | Base exception for domain invariants that were broken by a programming error. Expected failures use Result; this exception is reserved for states that should never | `src/CardioSense.Domain/Common/Entity.cs` |

### src/CardioSense.Domain/Interfaces

| File | Responsibility in plain words | Entry point or consumer |
|---|---|---|
| `HeartRateRange.cs` | Lowest and highest heart rate seen in a period. Returned by the time-series store so the doctor's patient list can show a daily min/max. | Owning module or runtime tooling |
| `IVitalsTimeSeriesStore.cs` | Storage contract for the high-frequency vitals time series (the Phase 1 -> Phase 2 seam). Application handlers depend only on this interface for vitals reads/writes, so SQL Server | `src/CardioSense.Application/Common/Interfaces/IAppDbContext.cs`, `src/CardioSense.Application/Doctors/Common/PatientListItemAssembler.cs` |

### src/CardioSense.Infrastructure/AiClient

| File | Responsibility in plain words | Entry point or consumer |
|---|---|---|
| `AiContracts.cs` | The JSON bodies exchanged with the Python AI service (POST /analyze and POST /report). These records ARE the wire contract the cardiosense-ai service must implement. Property names | Owning module or runtime tooling |
| `AiErrors.cs` | The two ways the AI service can fail from the backend's point of view. Both are "Unavailable" (HTTP 503 at the edge): either the service could not be reached/answered in | `src/CardioSense.Infrastructure/AiClient/HttpRiskAssessmentService.cs` |
| `AiServiceOptions.cs` | Settings of the connection to the Python AI service. The service is internal-only (never exposed to the internet), so these values point at its private | `src/CardioSense.Infrastructure/AiClient/HttpRiskAssessmentService.cs`, `src/CardioSense.Infrastructure/DependencyInjection.cs` |
| `HttpRiskAssessmentService.cs` | Implements IRiskAssessmentService by calling the Python AI service over internal HTTP/JSON. AssessAsync -> POST /analyze, GenerateReportNarrativeAsync -> POST /report. Any transport problem, | `src/CardioSense.Application/Common/Interfaces/IRiskAssessmentService.cs`, `src/CardioSense.Infrastructure/DependencyInjection.cs` |

### src/CardioSense.Infrastructure

| File | Responsibility in plain words | Entry point or consumer |
|---|---|---|
| `CardioSense.Infrastructure.csproj` | Declares CardioSense.Infrastructure.csproj settings or project metadata for CardioSense.Infrastructure. | Owning module or runtime tooling |
| `DependencyInjection.cs` | AddInfrastructure() - registers every concrete implementation behind the Application interfaces. One call from the Api's Program.cs wires the database, Identity, tokens, the vitals store, the AI | `Directory.Packages.props`, `src/CardioSense.Api/DependencyInjection.cs` |

### src/CardioSense.Infrastructure/Identity

| File | Responsibility in plain words | Entry point or consumer |
|---|---|---|
| `ApplicationUser.cs` | The login account stored in the Users table (ASP.NET Core Identity user + CardioSense fields). Identity owns e-mail, password hash and lockout; this class adds the role (which decides the profile | `src/CardioSense.Infrastructure/DependencyInjection.cs`, `src/CardioSense.Infrastructure/Identity/IdentityService.cs` |
| `IdentityErrors.cs` | Catalogue of account-level failures plus the translation of ASP.NET Identity errors into Errors. Identity reports problems as IdentityError codes; the rest of the system speaks Result/Error, so | `src/CardioSense.Infrastructure/Identity/IdentityService.cs`, `tests/CardioSense.Infrastructure.Tests/Identity/IdentityErrorsTests.cs` |
| `IdentityService.cs` | Implements IIdentityService on top of ASP.NET Core Identity (UserManager). Creates accounts, verifies credentials (with lockout), loads accounts by id and stores the language. | `src/CardioSense.Application/Common/Interfaces/IIdentityService.cs`, `src/CardioSense.Infrastructure/DependencyInjection.cs` |
| `JwtClaimNames.cs` | The single list of claim names stored in the access token. The token writer (JwtTokenService), the SignalR hub and the Api's ICurrentUser must agree on these | `src/CardioSense.Api/Authentication/CurrentUser.cs`, `src/CardioSense.Api/Authentication/JwtBearerSetup.cs` |
| `JwtOptions.cs` | Strongly-typed settings of the JWT access token and the refresh token lifetime. Bound from the "Jwt" configuration section and validated when the application starts, so a missing | `src/CardioSense.Api/Authentication/JwtBearerSetup.cs`, `src/CardioSense.Infrastructure/DependencyInjection.cs` |
| `JwtTokenService.cs` | Implements ITokenService - signed JWT access tokens and opaque refresh tokens. The access token is a short-lived HS256 JWT carrying sub, role and (per role) the patient or doctor | `src/CardioSense.Api/Authentication/JwtBearerSetup.cs`, `src/CardioSense.Application/Common/Interfaces/ITokenService.cs` |

### src/CardioSense.Infrastructure/Ingestion

| File | Responsibility in plain words | Entry point or consumer |
|---|---|---|
| `IngestionErrors.cs` | Reasons an incoming MQTT message is rejected before it reaches the application. The worker logs these codes and drops the message; nothing malformed is ever trusted or retried. | `src/CardioSense.Infrastructure/Ingestion/TelemetryMessageParser.cs` |
| `MqttIngestionService.cs` | Background worker that receives device messages from the MQTT broker and feeds the application. Connects (TLS by default), subscribes to devices/+/telemetry and devices/+/status, and keeps | `src/CardioSense.Infrastructure/DependencyInjection.cs` |
| `MqttOptions.cs` | Settings of the connection to the MQTT broker that devices publish to. TLS is on by default (port 8883) because medical readings must never cross a plain-text hop. | `src/CardioSense.Infrastructure/DependencyInjection.cs`, `src/CardioSense.Infrastructure/Ingestion/MqttIngestionService.cs` |
| `TelemetryMessageParser.cs` | Turns a raw MQTT message (topic + JSON text) into the matching application command. Topics: devices/{serial}/telemetry -> ProcessTelemetryCommand, devices/{serial}/status -> | `src/CardioSense.Infrastructure/Ingestion/MqttIngestionService.cs`, `tests/CardioSense.Infrastructure.Tests/Ingestion/TelemetryMessageParserTests.cs` |

### src/CardioSense.Infrastructure/Persistence

| File | Responsibility in plain words | Entry point or consumer |
|---|---|---|
| `AppDbContext.cs` | The EF Core database context - implements IAppDbContext and hosts the Identity tables. Application handlers see only IAppDbContext (one DbSet per entity, no repositories). The context | `src/CardioSense.Api/DependencyInjection.cs`, `src/CardioSense.Application/Common/Interfaces/IAppDbContext.cs` |
| `AppDbContextFactory.cs` | Lets the "dotnet ef" tools create the context without starting the whole application. Migrations can be generated and applied with only this project (no Api needed): | `src/CardioSense.Infrastructure/Persistence/DatabaseInitializer.cs` |
| `DatabaseInitializer.cs` | Start-up database tasks called by the Api host: create the schema and seed demo devices. Device pairing works only for devices that already exist in the inventory (there is no admin role | `src/CardioSense.Api/WebApplicationExtensions.cs` |
| `UniqueViolation.cs` | Tells the Api whether a failed save was caused by a unique-index violation. Unique indexes (national id, license number, serial number, device-per-patient, primary doctor) | `src/CardioSense.Api/Http/ApiErrors.cs`, `src/CardioSense.Api/Middleware/GlobalExceptionHandler.cs` |

### src/CardioSense.Infrastructure/Persistence/Configurations

| File | Responsibility in plain words | Entry point or consumer |
|---|---|---|
| `AlertConfiguration.cs` | Maps Alert to the Alerts table. Two access paths drive the indexes: the alerts timeline (newest first per patient) and the | Owning module or runtime tooling |
| `ApplicationUserConfiguration.cs` | Maps the login account to the Users table. Identity already maps e-mail, password hash and lockout; this adds the table name from the | Owning module or runtime tooling |
| `AuditLogEntryConfiguration.cs` | Maps AuditLogEntry to the AuditLogs table. Answers "who looked at this record, and when" - so the index is (EntityType, EntityId, Timestamp). | Owning module or runtime tooling |
| `DeviceConfiguration.cs` | Maps Device to the Devices table. The serial number is unique. PatientId is nullable (inventory devices belong to nobody) and, among | Owning module or runtime tooling |
| `DoctorConfiguration.cs` | Maps Doctor to the Doctors table. The license number is unique (one profile per real doctor) and each login account owns at most one | Owning module or runtime tooling |
| `DoctorPatientAssignmentConfiguration.cs` | Maps DoctorPatientAssignment to the DoctorPatientAssignments table - the base of all access rules. A doctor can supervise a patient only once; a patient has at most one primary doctor (the one the | Owning module or runtime tooling |
| `EcgFeatureSetConfiguration.cs` | Maps EcgFeatureSet to the EcgFeatureSets table. Stores the processed features (PR, QRS, QT, QTc) - never the raw waveform. Queried as "latest per | Owning module or runtime tooling |
| `ElectrodeVerificationConfiguration.cs` | Maps ElectrodeVerification to the ElectrodeVerifications table. Results of the camera-based electrode placement check; the dashboard reads the newest one per | Owning module or runtime tooling |
| `EntityConfigurationExtensions.cs` | Shared EF Core mapping helpers so the thirteen entity configurations stay short and consistent. Every entity repeats the same three decisions - table + client-generated key, enums stored as | Owning module or runtime tooling |
| `MedicalHistoryEntryConfiguration.cs` | Maps MedicalHistoryEntry to the MedicalHistoryEntries table. One row per diagnosis / medication / condition so the UI can list and edit them individually. | Owning module or runtime tooling |
| `PatientConfiguration.cs` | Maps Patient to the Patients table. National id, file number and the owning account are each unique. The file number (CS-0042) is | `src/CardioSense.Infrastructure/Persistence/AppDbContext.cs` |
| `RefreshTokenConfiguration.cs` | Maps RefreshToken to the RefreshTokens table. Only the hash of a token is stored. The hash is unique because every refresh request finds its row | Owning module or runtime tooling |
| `RiskAssessmentConfiguration.cs` | Maps RiskAssessment to the RiskAssessments table. One assessment per feature set at most (a window the AI could not assess has none). The doctor- | Owning module or runtime tooling |
| `VitalsSampleConfiguration.cs` | Maps VitalsSample to the VitalsSamples table (the Phase 1 home of the vitals time series). Almost pure appends, read as "latest" or "a time window of one patient", so the one index that | Owning module or runtime tooling |

### src/CardioSense.Infrastructure/Realtime

| File | Responsibility in plain words | Entry point or consumer |
|---|---|---|
| `MonitoringHub.cs` | The SignalR hub (/hubs/monitoring) browsers connect to for live ECG, vitals, status and alerts. On connect a patient joins their own group and a doctor joins their personal group, both taken from | `src/CardioSense.Api/Authentication/CurrentUserHubFilter.cs`, `src/CardioSense.Api/Authentication/JwtBearerSetup.cs` |
| `MonitoringNames.cs` | The single source of truth for SignalR group names and client event names. Per patient there are two audiences: the patient's own connections (patient-{id}) and the doctors | `src/CardioSense.Infrastructure/Realtime/MonitoringHub.cs`, `src/CardioSense.Infrastructure/Realtime/SignalRRealtimeNotifier.cs` |
| `SignalRRealtimeNotifier.cs` | Implements IRealtimeNotifier - pushes live data to browsers through the monitoring hub. Vitals, ECG samples and alerts go to everyone watching the patient (the patient and the viewing | `src/CardioSense.Application/Common/Interfaces/IRealtimeNotifier.cs`, `src/CardioSense.Infrastructure/DependencyInjection.cs` |

### src/CardioSense.Infrastructure/TimeSeries

| File | Responsibility in plain words | Entry point or consumer |
|---|---|---|
| `SqlVitalsTimeSeriesStore.cs` | Phase 1 implementation of IVitalsTimeSeriesStore - the vitals time series lives in SQL Server. Handlers depend only on the interface, so replacing this class with an InfluxDB/TimescaleDB store | `src/CardioSense.Domain/Interfaces/IVitalsTimeSeriesStore.cs`, `src/CardioSense.Infrastructure/DependencyInjection.cs` |

### tests/CardioSense.Api.IntegrationTests

| File | Responsibility in plain words | Entry point or consumer |
|---|---|---|
| `CardioSense.Api.IntegrationTests.csproj` | Exercises Cardio Sense behavior with test fixtures. | Test runner |

### tests/CardioSense.Api.IntegrationTests/Scenarios

| File | Responsibility in plain words | Entry point or consumer |
|---|---|---|
| `AccessControlTests.cs` | Verifies who may see and do what, through the real HTTP stack and database. The rules that protect patient data: a doctor sees only assigned patients, patients cannot use | Test runner |
| `AuthFlowTests.cs` | Verifies sign-up, refresh, logout and the refresh cookie end to end. The refresh token must live only in an httpOnly, Secure, path-limited cookie and never in a JSON | Test runner |
| `DevicePairingTests.cs` | Verifies device pairing against the demo inventory seeded at start-up. Only devices in inventory can be paired, one device belongs to one patient, and the database's | Test runner |
| `HealthAndErrorShapeTests.cs` | Verifies the probes, the standard security headers and the shape of error responses. A consistent error shape is part of the contract with the frontend: every failure is | Test runner |
| `MonitoringHubTests.cs` | Verifies the SignalR hub with real authenticated connections. A doctor may subscribe to a patient's live stream only when assigned to that patient - the same | Test runner |

### tests/CardioSense.Api.IntegrationTests/Support

| File | Responsibility in plain words | Entry point or consumer |
|---|---|---|
| `Accounts.cs` | Creates patients and doctors through the real sign-up endpoints and hands back a signed-in client. All tests share one database, so every account gets unique e-mail, national id and license number | `tests/CardioSense.Api.IntegrationTests/Scenarios/AccessControlTests.cs`, `tests/CardioSense.Api.IntegrationTests/Scenarios/AuthFlowTests.cs` |
| `ApiCollection.cs` | Makes every integration test class share one ApiFactory (one container, one API instance). xUnit creates the fixture once per collection; every class tagged with the collection reuses it. | `tests/CardioSense.Api.IntegrationTests/Scenarios/AccessControlTests.cs`, `tests/CardioSense.Api.IntegrationTests/Scenarios/AuthFlowTests.cs` |
| `ApiFactory.cs` | Starts the real API in memory on top of a throw-away SQL Server container. One container and one API instance are shared by all integration tests (starting SQL Server is | `tests/CardioSense.Api.IntegrationTests/Scenarios/AccessControlTests.cs`, `tests/CardioSense.Api.IntegrationTests/Scenarios/AuthFlowTests.cs` |
| `StubAiService.cs` | Replaces the Python AI service so integration tests need no network. Only the AI is faked; everything else in the integration tests is real. | `tests/CardioSense.Api.IntegrationTests/Support/ApiFactory.cs` |

### tests/CardioSense.Application.Tests

| File | Responsibility in plain words | Entry point or consumer |
|---|---|---|
| `CardioSense.Application.Tests.csproj` | Exercises Cardio Sense behavior with test fixtures. | Test runner |

### tests/CardioSense.Application.Tests/Entities

| File | Responsibility in plain words | Entry point or consumer |
|---|---|---|
| `AlertTests.cs` | Verifies the alert rules that protect patients: a critical alert is released only by a doctor. The rule lives on the entity, so it holds no matter which use case touches the alert, now or in a | Test runner |
| `DeviceTests.cs` | Verifies device pairing and "last seen" rules. A device belongs to one patient at a time (silently re-pairing could stop someone's monitoring), | Test runner |
| `PatientTests.cs` | Verifies age handling: the signup form collects an age, the database stores a date of birth. The form's age is converted to an approximate birth date, and age is always recomputed from the | Test runner |
| `RefreshTokenTests.cs` | Verifies token reuse detection: theft is told apart from a harmless double request. A browser can send the same refresh cookie twice within moments (two tabs, React StrictMode). A | Test runner |
| `ResultTests.cs` | Verifies the Result type that every use case returns instead of throwing for expected failures. A failed result must never hand out a value, and a result cannot be both successful and carry an | Test runner |
| `RiskAssessmentTests.cs` | Verifies the most important safety rule: a Critical status never drops automatically. While a critical alert is open the stored status stays Critical even if the AI later says | Test runner |
| `VitalsSampleTests.cs` | Verifies the sensor sanity limits on a vitals reading. The limits are deliberately wide (heart rate 20-300, SpO2 50-100) so genuine pathological values | Test runner |

### tests/CardioSense.Application.Tests/Pipeline

| File | Responsibility in plain words | Entry point or consumer |
|---|---|---|
| `AuthorizationPipelineTests.cs` | Verifies "who may see which patient" through the real mediator pipeline. A patient sees only themself, a doctor sees only assigned patients, anyone else sees nothing. The | Test runner |
| `GetMyPatientsTests.cs` | Verifies the doctor's patient list: it contains assigned patients only. "42 active patients" on the doctor dashboard must mean the patients assigned to THIS doctor, not | Test runner |
| `PairDeviceTests.cs` | Verifies device pairing rules. Only devices that exist in inventory can be paired (so the broker cannot create devices), one | Test runner |
| `ProcessTelemetryTests.cs` | Verifies the core use case: a device reading in; stored data, AI assessment, alerts, pushes out. Covers the rules a clinician depends on - unknown devices are not trusted, a reading is kept even | Test runner |
| `RefreshTokenPipelineTests.cs` | Verifies session renewal: rotation, the concurrent-request leeway and theft detection. Every refresh replaces the token. A just-replaced token still works for a few seconds (two tabs); | Test runner |
| `RegisterPatientTests.cs` | Verifies patient sign-up end to end: account, profile, history, optional links, and the session. Sign-up must be all-or-nothing and safe by default: a duplicate national id is a conflict, a doctor | Test runner |
| `ValidationPipelineTests.cs` | Verifies that invalid input is rejected before any handler runs, with errors per field. The frontend shows each message under the matching form control, so the keys must be the camelCase | Test runner |

### tests/CardioSense.Application.Tests/Support

| File | Responsibility in plain words | Entry point or consumer |
|---|---|---|
| `ApplicationTestHost.cs` | Builds the real Application (mediator pipeline, validators, handlers) on top of fakes. Tests send commands and queries through ISender exactly like an endpoint does, so authorization, | `tests/CardioSense.Application.Tests/Entities/AlertTests.cs`, `tests/CardioSense.Application.Tests/Entities/DeviceTests.cs` |
| `FakeCurrentUser.cs` | Lets a test sign in as a given patient or doctor (or nobody). Authorization in Application depends only on ICurrentUser, so changing these four properties is | `tests/CardioSense.Application.Tests/Support/ApplicationTestHost.cs` |
| `FixedTimeProvider.cs` | A clock the test controls. Handlers read the time from TimeProvider, so a test that fixes "now" can check expiry windows, | `tests/CardioSense.Application.Tests/Support/ApplicationTestHost.cs`, `tests/CardioSense.Infrastructure.Tests/Identity/JwtTokenServiceTests.cs` |
| `InMemoryVitalsStore.cs` | IVitalsTimeSeriesStore backed by a list. Handlers must work with any store behind the Domain interface (SQL today, a time-series database | `tests/CardioSense.Application.Tests/Support/ApplicationTestHost.cs` |
| `RecordingNotifier.cs` | IRealtimeNotifier that records what would have been pushed to browsers. Tests assert on the pushes - especially that a patient never receives the clinical risk numbers - | `tests/CardioSense.Application.Tests/Support/ApplicationTestHost.cs` |
| `Seed.cs` | Small factories for valid domain objects, so a test states only what matters to it. Every entity has strict invariants (non-empty names, valid ranges); building them by hand in every | `tests/CardioSense.Application.Tests/Entities/PatientTests.cs`, `tests/CardioSense.Application.Tests/Pipeline/AuthorizationPipelineTests.cs` |
| `StubIdentityService.cs` | IIdentityService over a dictionary, in place of ASP.NET Core Identity. Registration and login handlers only need "create an account", "check a password" and "find an | `tests/CardioSense.Application.Tests/Support/ApplicationTestHost.cs` |
| `StubRiskAssessmentService.cs` | IRiskAssessmentService whose answer the test chooses. The Python AI service is not part of Application tests. A test sets NextResult to a status (or to | `tests/CardioSense.Application.Tests/Pipeline/ProcessTelemetryTests.cs`, `tests/CardioSense.Application.Tests/Support/ApplicationTestHost.cs` |
| `StubTokenService.cs` | ITokenService with predictable, readable tokens. Real JWT signing is an Infrastructure concern (tested there). Here a token only has to be a | `tests/CardioSense.Application.Tests/Support/ApplicationTestHost.cs` |
| `TestDbContext.cs` | The in-memory implementation of IAppDbContext used by use-case tests. Application only knows IAppDbContext, so tests need nothing from Infrastructure: a plain DbContext | `tests/CardioSense.Application.Tests/Support/ApplicationTestHost.cs` |

### tests/CardioSense.Infrastructure.Tests/AiClient

| File | Responsibility in plain words | Entry point or consumer |
|---|---|---|
| `HttpRiskAssessmentServiceTests.cs` | Verifies the HTTP client for the Python AI service against a fake HTTP handler (no network). The client's safety rule: it never invents an answer. Anything unusable - the service down, an | Test runner |

### tests/CardioSense.Infrastructure.Tests

| File | Responsibility in plain words | Entry point or consumer |
|---|---|---|
| `CardioSense.Infrastructure.Tests.csproj` | Exercises Cardio Sense behavior with test fixtures. | Test runner |

### tests/CardioSense.Infrastructure.Tests/Identity

| File | Responsibility in plain words | Entry point or consumer |
|---|---|---|
| `IdentityErrorsTests.cs` | Verifies how ASP.NET Core Identity failures become application errors. The frontend reacts to the category (a taken e-mail is a conflict, a weak password is a validation | Test runner |
| `JwtTokenServiceTests.cs` | Verifies access-token contents and refresh-token generation. The access token is what every authorization decision reads, so its claims must be exactly what the | Test runner |

### tests/CardioSense.Infrastructure.Tests/Ingestion

| File | Responsibility in plain words | Entry point or consumer |
|---|---|---|
| `TelemetryMessageParserTests.cs` | Verifies how raw MQTT messages become commands - the contract the firmware must follow. The serial number is taken from the topic, never from the payload (a device cannot claim to be | Test runner |

### tests/CardioSense.Infrastructure.Tests/Persistence

| File | Responsibility in plain words | Entry point or consumer |
|---|---|---|
| `PersistenceModelTests.cs` | Verifies the shape of the EF Core model without connecting to any database. Building the model needs no connection, yet it reveals the mistakes that hurt most in production: | Test runner |
| `UniqueViolationTests.cs` | Verifies that only genuine duplicate-key failures are reported as unique violations. The Api turns a unique violation into 409 Conflict and everything else into 500, so the detector | Test runner |

### tests/CardioSense.Infrastructure.Tests/Support

| File | Responsibility in plain words | Entry point or consumer |
|---|---|---|
| `FixedTimeProvider.cs` | A clock the test controls. Token expiry is computed from TimeProvider, so a fixed "now" makes the expected expiry exact. | `tests/CardioSense.Application.Tests/Support/ApplicationTestHost.cs`, `tests/CardioSense.Application.Tests/Support/FixedTimeProvider.cs` |

## Risk service

Root: `back-end/back/cardiosense-ai`. 17 listed files and 17 inventory entries.

```text
.dockerignore
.gitignore
app/
  __init__.py
  inference.py
  main.py
  models/
    __init__.py
    risk_classifier.py
  report.py
  schemas.py
Dockerfile
README.md
requirements-dev.txt
requirements.txt
tests/
  __init__.py
  test_api.py
  test_report.py
  test_risk_classifier.py
```

### .

| File | Responsibility in plain words | Entry point or consumer |
|---|---|---|
| `.dockerignore` | Declares .dockerignore settings or project metadata for cardiosense-ai. | Owning module or runtime tooling |
| `.gitignore` | Declares .gitignore settings or project metadata for cardiosense-ai. | Owning module or runtime tooling |
| `Dockerfile` | Builds the service image and defines its entry command. | Owning module or runtime tooling |
| `README.md` | Declares README.md settings or project metadata for cardiosense-ai. | Owning module or runtime tooling |
| `requirements-dev.txt` | Adds HTTP test tooling. | Owning module or runtime tooling |
| `requirements.txt` | Declares runtime Python dependency ranges. | Owning module or runtime tooling |

### app

| File | Responsibility in plain words | Entry point or consumer |
|---|---|---|
| `__init__.py` | Declares the Python package boundary. | Owning module or runtime tooling |
| `inference.py` | Defines `get_classifier`. | Owning module or runtime tooling |
| `main.py` | Defines `_to_context`, `health`, `analyze`, `create_report`. | Owning module or runtime tooling |
| `report.py` | Defines `build_narrative`, `_heart_rate_sentence`, `_alerts_sentence`. | Owning module or runtime tooling |
| `schemas.py` | Declares the Python package boundary. | Owning module or runtime tooling |

### app/models

| File | Responsibility in plain words | Entry point or consumer |
|---|---|---|
| `__init__.py` | Declares the Python package boundary. | Owning module or runtime tooling |
| `risk_classifier.py` | Defines `_crosses`, `_is_borderline`, `_has_cardiac_history`, `assess`, `_context_points`, `_confidence`. | Owning module or runtime tooling |

### tests

| File | Responsibility in plain words | Entry point or consumer |
|---|---|---|
| `__init__.py` | Exercises __init__ behavior with test fixtures. | Test runner |
| `test_api.py` | Exercises test_api behavior with test fixtures. | Test runner |
| `test_report.py` | Exercises test_report behavior with test fixtures. | Test runner |
| `test_risk_classifier.py` | Exercises test_risk_classifier behavior with test fixtures. | Test runner |

## Generated and external artifacts

- EF migration, designer and snapshot live in `Persistence/Migrations`. Read them for executable schema and SQL column definitions.
- npm lockfile provides exact resolved Web versions. Build output, caches and dependency installations are excluded.
- Hardware firmware and the BLE gateway have no verified implementation source root in the workspace. Their proposed boundaries are in [IoT architecture](iot-device-architecture.md).
- Source inventories omit runtime environment files and Vercel deployment snapshots.
