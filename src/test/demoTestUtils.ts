/**
 * File: src/test/demoTestUtils.ts
 * Layer: test support (never imported by application code)
 * Purpose: Shared helpers for the demo-backend tests: call the in-browser router like the Axios adapter would,
 *          sign in as a seeded account, and pin the UI language.
 * Logic: Tests run against the real router and the real (localStorage-backed) database; every test starts from a
 *        freshly reset, freshly seeded database so they cannot influence each other.
 * Implementation: thin wrappers around handleDemoRequest / resetDemoData.
 */
import { DEMO_PASSWORD } from '@/shared/demo/demoConstants';
import { handleDemoRequest } from '@/shared/demo/demoServer';
import { resetDemoData } from '@/shared/demo/demoStorage';
import type { DemoMethod, DemoResponse } from '@/shared/demo/demoTypes';
import type { AuthResponse } from '@/shared/types/api';

/** A fixed "current time" (Sunday 18 Oct 2026, 12:00 UTC) so time-dependent output is reproducible. */
export const FIXED_NOW = Date.UTC(2026, 9, 18, 12, 0, 0);

export const DOCTOR_EMAIL = 'doctor@cardiosense.demo';
export const STABLE_PATIENT_EMAIL = 'patient@cardiosense.demo';
export const CAUTION_PATIENT_EMAIL = 'patient.caution@cardiosense.demo';
export const CRITICAL_PATIENT_EMAIL = 'patient.critical@cardiosense.demo';

interface CallOptions {
  body?: unknown;
  token?: string | null;
  query?: Record<string, string>;
  nowMs?: number;
}

/** Sends one request to the in-browser router. */
export function call(method: DemoMethod, path: string, options: CallOptions = {}): Promise<DemoResponse> {
  return handleDemoRequest({
    method,
    path,
    query: options.query ?? {},
    body: options.body,
    authorization: options.token ? `Bearer ${options.token}` : null,
    nowMs: options.nowMs ?? FIXED_NOW,
  });
}

/** Signs in and returns the login response; throws if the sign-in is rejected. */
export async function loginAs(
  email: string,
  password: string = DEMO_PASSWORD,
  nowMs: number = FIXED_NOW,
): Promise<AuthResponse> {
  const response = await call('POST', '/auth/login', { body: { email, password }, nowMs });
  if (response.status !== 200) throw new Error(`Login failed for ${email}: HTTP ${response.status}`);
  return response.data as AuthResponse;
}

/** Pins the language the demo backend localises its text to (it reads <html lang>). */
export function setUiLanguage(language: 'ar' | 'en'): void {
  document.documentElement.setAttribute('lang', language);
}

/** Wipes the demo database and session and selects English; call from beforeEach. */
export function freshDemo(): void {
  resetDemoData();
  setUiLanguage('en');
}
