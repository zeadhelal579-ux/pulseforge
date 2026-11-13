/// <reference types="vite/client" />

interface ImportMetaEnv {
  // Vite's own built-in fields (normally supplied by vite/client's ambient
  // types via declaration merging — declared explicitly here too so this
  // file is self-contained and signalrClient.ts's `import.meta.env.DEV`
  // check resolves even before `npm install` has fetched Vite's types).
  readonly DEV: boolean;
  readonly PROD: boolean;
  readonly MODE: string;
  readonly SSR: boolean;

  readonly VITE_API_BASE_URL: string;
  readonly VITE_SIGNALR_HUB_URL: string;
  readonly VITE_DEFAULT_LANGUAGE: 'ar' | 'en';
}

interface ImportMeta {
  readonly env: ImportMetaEnv;
}
