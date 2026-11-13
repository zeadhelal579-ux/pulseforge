/**
 * File: src/shared/lib/env.ts
 * Layer: shared/lib
 * Purpose: Single, typed entry point for every Vite environment variable the app reads.
 * Logic:
 *  - Empty or whitespace-only values count as "not set", so a blank `VITE_API_BASE_URL=` line in a
 *    .env file can never silently become an empty base URL.
 *  - Demo mode is ON when VITE_DEMO_MODE is "true", OFF when it is "false", and, when the flag is
 *    absent, ON only if no API URL has been configured. A bare Vercel deployment (no env vars) therefore
 *    works out of the box, while a developer who copies .env.example talks to the live backend.
 * Implementation: plain constants evaluated once at module load (Vite inlines import.meta.env at build time).
 */

/** Trims a raw env value and maps empty strings to undefined. */
function readEnv(value: string | undefined): string | undefined {
  const trimmed = value?.trim();
  return trimmed ? trimmed : undefined;
}

const configuredApiBaseUrl = readEnv(import.meta.env.VITE_API_BASE_URL);
const configuredHubUrl = readEnv(import.meta.env.VITE_SIGNALR_HUB_URL);

/** Base URL of the ASP.NET Core Web API (falls back to the same-origin `/api`). */
export const API_BASE_URL: string = configuredApiBaseUrl ?? '/api';

/** URL of the SignalR monitoring hub (falls back to the same-origin `/hubs/monitoring`). */
export const SIGNALR_HUB_URL: string = configuredHubUrl ?? '/hubs/monitoring';

/** UI language used when neither a saved choice nor the browser language is supported. */
export const DEFAULT_LANGUAGE: 'ar' | 'en' =
  readEnv(import.meta.env.VITE_DEFAULT_LANGUAGE) === 'en' ? 'en' : 'ar';

/** Decides whether the in-browser demo backend replaces the real API. */
function resolveDemoMode(): boolean {
  const flag = readEnv(import.meta.env.VITE_DEMO_MODE)?.toLowerCase();
  if (flag === 'true' || flag === '1') return true;
  if (flag === 'false' || flag === '0') return false;
  return configuredApiBaseUrl === undefined;
}

/** True when requests are answered by the in-browser demo backend instead of a real server. */
export const IS_DEMO_MODE: boolean = resolveDemoMode();
