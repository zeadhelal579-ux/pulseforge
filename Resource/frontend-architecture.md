# Frontend architecture

The maintained local source is `front-end/CardioSense-Frontend-fixed`. `front Deploy` is a separate Vercel snapshot with an in-browser demo. Synchronization between the two has no documented release workflow. See the [file map](architecture-file-map.md).

## Structure and dependency rules

`main.tsx` mounts React and applies persisted theme. `app/App.tsx` wires providers. `app/router.tsx` associates layouts and access guards with pages. `features/` owns page UI, form validation, API adapters and query hooks by domain. `shared/` owns reusable controls, formatters, HTTP, SignalR and wire types. App-level AuthProvider supplies token callbacks to shared Axios without a reverse feature import.

| Page | Implementation guide | State and actions |
|---|---|---|
| P01 | AuthPage, AuthLayout, LoginForm, SignupForm, role toggle and Zod schemas. API-01–05 | Field errors, busy state and role redirect. Logout clears cached account data |
| P02 | PatientDashboardPage and PatientDetailContent. API-06 and API-08–10 plus RT-01 | Initial REST snapshot, live vitals and ECG, no-data and disconnect states, doctor contact when assigned |
| P03 | PatientListPage, WardStatsCards, PatientListFilters, PatientListItem. API-20–22 | Debounced search, filter selection, paging, assignment modal and list refresh |
| P04 | PatientDetailPage and PatientDetailChrome around shared detail content. API-11–17 and RT-01 | 403/404 state, group subscription, doctor-only score, freeze and manual event. API resolve exists and needs a UI |
| P05 | SettingsPage, LanguagePreference and DevicePairingForm. API-07, 19, 23 and 24 | Role-specific layout, language persistence, validation and device inventory or ownership errors |

## State and rendering

React Query owns REST snapshots. Zustand owns session and theme. Access token stays in memory. Refresh cookie restores the session on reload. Axios shares in-flight refresh and retries a protected call once. Session changes clear query data. SignalRProvider scopes the connection to authentication. The waveform hook stores samples in a ring buffer and Canvas renders via requestAnimationFrame. After 15 seconds without samples the hook resets to idle. Profile vitals can still be last known values. Explicit age and source labeling throughout the UI is a pilot task.

i18next applies lang and dir from locale. The dictionaries live in locales/en and locales/ar. Global CSS provides light and dark tokens. Verify both languages, themes, keyboard interaction and error rendering when adding a screen. Axe was run on the tested patient and doctor detail screens. See [audit evidence](../audit-report/REPORT.md).

## Extension and open items

Add a feature by defining a contract, API adapter and query keys, then connecting a guarded page and tests. Password recovery, alert resolution UI and full browser/accessibility coverage remain open. [Flows](flows.md) defines cross-layer behavior.
