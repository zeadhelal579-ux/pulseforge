import { BrowserRouter } from 'react-router-dom';
import { I18nProvider } from '@/app/providers/I18nProvider';
import { QueryProvider } from '@/app/providers/QueryProvider';
import { AuthProvider } from '@/app/providers/AuthProvider';
import { DemoRuntime } from '@/app/providers/DemoRuntime';
import { SignalRProvider } from '@/app/providers/SignalRProvider';
import { AppRouter } from '@/app/router';

/**
 * Provider order matters here:
 *  1. I18nProvider — translations must exist before anything renders text.
 *  2. QueryProvider — REST caching layer, no dependency on auth to exist.
 *     DemoRuntime (demo mode only, renders nothing) lives inside it because it needs the QueryClient.
 *  3. AuthProvider — resolves the session (needs QueryClient-free plain
 *     axios/fetch calls internally, but renders a gate while bootstrapping).
 *  4. SignalRProvider — needs AuthProvider's session state (the access token)
 *     to decide whether to open a socket at all.
 *  5. BrowserRouter + AppRouter — routes read from AuthProvider's authStore.
 */
export function App() {
  return (
    <I18nProvider>
      <QueryProvider>
        <DemoRuntime />
        <AuthProvider>
          <SignalRProvider>
            <BrowserRouter>
              <AppRouter />
            </BrowserRouter>
          </SignalRProvider>
        </AuthProvider>
      </QueryProvider>
    </I18nProvider>
  );
}
