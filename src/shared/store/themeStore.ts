import { create } from 'zustand';
import { persist } from 'zustand/middleware';

export type ThemeMode = 'dark' | 'light';

interface ThemeState {
  mode: ThemeMode;
  toggleTheme: () => void;
  setTheme: (mode: ThemeMode) => void;
}

/**
 * `dark` is the CardioSense default (matches every reviewed mockup and the
 * auth prototype). `light` is the "daytime ward" variant anticipated by
 * frontend-architecture.md §8's theming notes.
 */
export const useThemeStore = create<ThemeState>()(
  persist(
    (set, get) => ({
      mode: 'dark',
      toggleTheme: () => set({ mode: get().mode === 'dark' ? 'light' : 'dark' }),
      setTheme: (mode) => set({ mode }),
    }),
    { name: 'cardiosense-theme' },
  ),
);

/**
 * Applies the current theme to <html data-theme="..."> so the CSS variable
 * overrides in styles/globals.css take effect. Called once on boot (see
 * app/App.tsx) and again from ThemeToggle on every click.
 */
export function applyThemeToDocument(mode: ThemeMode): void {
  document.documentElement.setAttribute('data-theme', mode);
}
