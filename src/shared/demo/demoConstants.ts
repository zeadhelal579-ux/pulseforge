/**
 * File: src/shared/demo/demoConstants.ts
 * Layer: shared/demo (in-browser demo backend)
 * Purpose: Every tunable number and storage key of the demo backend in one place.
 * Logic: Nothing here is computed; modules import what they need so a value is never duplicated.
 * Implementation: plain `as const` / primitive exports with a short note on why each value exists.
 */

/** localStorage key holding the whole demo database (JSON). Bump the suffix when the shape changes. */
export const DEMO_STORAGE_KEY = 'cardiosense.demo.db.v1';

/** localStorage key emulating the httpOnly refresh cookie of the real backend. */
export const DEMO_SESSION_KEY = 'cardiosense.demo.session.v1';

/** Schema version stored inside the database; a mismatch triggers a fresh seed. */
export const DEMO_DB_VERSION = 1;

/** Public password shared by every seeded demo account (it is shown on the login page on purpose). */
export const DEMO_PASSWORD = 'Demo@1234';

/** Artificial latency range for demo requests, so loading states and spinners behave realistically. */
export const DEMO_REQUEST_LATENCY_MS = { min: 140, max: 360 } as const;

/** Latency of the silent token refresh; short because it happens on every page load. */
export const DEMO_REFRESH_LATENCY_MS = 40;

/** Access token lifetime; mirrors the real backend (about 15 minutes) so the 401 -> refresh path is exercised. */
export const ACCESS_TOKEN_TTL_MS = 15 * 60 * 1000;

/** How long the emulated refresh "cookie" stays valid. */
export const REFRESH_SESSION_TTL_MS = 7 * 24 * 60 * 60 * 1000;

/** Page size of the doctor's patient list; mirrors PATIENT_LIST_PAGE_SIZE on the frontend. */
export const DEMO_PAGE_SIZE = 5;

/** Ward capacity shown in the "Total Active Patients 12/50" tile. */
export const DEMO_WARD_CAPACITY = 50;

/** Denominator of the critical-cases progress bar. */
export const DEMO_CRITICAL_CAPACITY = 10;

/** A device counts as "connected" when it was seen within this window. */
export const DEMO_DEVICE_LIVE_WINDOW_MS = 2 * 60 * 1000;

/** Sample rate of the simulated ECG stream; matches ECG_SAMPLE_RATE_HZ used by the waveform component. */
export const DEMO_ECG_SAMPLE_RATE_HZ = 250;

/** Interval between simulated heart-rate / SpO2 ticks pushed over the demo hub. */
export const DEMO_VITALS_TICK_MS = 2000;

/** Interval between simulated risk-status pushes over the demo hub. */
export const DEMO_RISK_TICK_MS = 10_000;

/** National ID of the seeded patient who is NOT linked to the demo doctor yet (used by "Add New Patient"). */
export const DEMO_UNASSIGNED_NATIONAL_ID = '29000000000099';
