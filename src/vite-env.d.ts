/// <reference types="vite/client" />

/**
 * Typed shape of the VITE_* variables this app reads (see src/shared/lib/env.ts).
 * Every variable is optional: an unset variable simply falls back to a default there.
 * Vite's own built-in fields (DEV, PROD, MODE, ...) come from `vite/client` above.
 */
interface ImportMetaEnv {
  readonly VITE_API_BASE_URL?: string;
  readonly VITE_SIGNALR_HUB_URL?: string;
  readonly VITE_DEFAULT_LANGUAGE?: 'ar' | 'en';
  readonly VITE_DEMO_MODE?: string;
}

interface ImportMeta {
  readonly env: ImportMetaEnv;
}
