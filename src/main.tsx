import { StrictMode } from 'react';
import { createRoot } from 'react-dom/client';
import { App } from '@/app/App';
import { applyThemeToDocument, useThemeStore } from '@/shared/store/themeStore';
import '@/styles/globals.css';

const rootElement = document.getElementById('root');
if (!rootElement) {
  throw new Error('Root element "#root" was not found in index.html.');
}

// Apply the persisted theme (zustand `persist` → localStorage) to <html
// data-theme> before the first paint. Without this, a returning user whose
// last choice was "light" would still see a dark first paint on every
// reload — ThemeToggle only ever applied the theme reactively on click, not
// on boot — and the toggle button would then visually disagree with what's
// actually on screen until clicked (twice) to resync.
applyThemeToDocument(useThemeStore.getState().mode);

createRoot(rootElement).render(
  <StrictMode>
    <App />
  </StrictMode>,
);
