import axios, { type InternalAxiosRequestConfig } from 'axios';
import { demoAdapter } from '@/shared/demo/demoAdapter';
import { API_BASE_URL, IS_DEMO_MODE } from '@/shared/lib/env';

/**
 * Demo mode swaps only the transport: requests are answered by the in-browser demo backend (see
 * shared/demo/). Everything else in this file (auth header, 401 -> refresh -> retry) runs unchanged, so the
 * demo exercises the same code paths as the real API. The spread is empty outside demo mode.
 */
const transport = IS_DEMO_MODE ? { adapter: demoAdapter } : {};

/**
 * Axios instance every feature's `api/*.ts` module calls through.
 *
 * `withCredentials: true` is required so the browser sends the httpOnly
 * refresh-token cookie (backend-database-architecture.md §8) on the
 * `/auth/refresh` call below — it is NEVER readable from JS, only sent
 * automatically by the browser to same-site requests.
 */
export const apiClient = axios.create({
  baseURL: API_BASE_URL,
  withCredentials: true,
  ...transport,
});

// ---------------------------------------------------------------------------
// Auth wiring — injected by app/providers/AuthProvider.tsx at startup.
//
// Why indirection instead of importing authStore directly: this file lives in
// shared/lib, and frontend-architecture.md §4 states shared/ must never import
// from features/ (that would invert the intended dependency direction, where
// features depend on shared and not vice-versa). AuthProvider lives in app/,
// which is allowed to depend on both, so it plays matchmaker at boot time.
// ---------------------------------------------------------------------------

interface ApiClientAuthConfig {
  /** Reads the current in-memory access token (or null if signed out). */
  getAccessToken: () => string | null;
  /** Called with the freshly-issued access token after a successful refresh. */
  onTokenRefreshed: (newAccessToken: string) => void;
  /** Called when the refresh call itself fails — session is unrecoverable. */
  onAuthFailure: () => void;
}

let authConfig: ApiClientAuthConfig | null = null;

export function configureApiClientAuth(config: ApiClientAuthConfig): void {
  authConfig = config;
}

// Attach the current access token to every outgoing request.
apiClient.interceptors.request.use((config: InternalAxiosRequestConfig) => {
  const token = authConfig?.getAccessToken();
  if (token) {
    config.headers.set('Authorization', `Bearer ${token}`);
  }
  return config;
});

/**
 * Ensures concurrent 401s trigger exactly one `/auth/refresh` call instead of
 * a stampede — every request that arrives while a refresh is already in
 * flight awaits the same promise instead of starting its own.
 */
let refreshInFlight: Promise<string> | null = null;

async function refreshAccessToken(): Promise<string> {
  // If a refresh is already in flight, hand back that same promise instead
  // of starting a second one.
  const existingRefresh = refreshInFlight;
  if (existingRefresh) return existingRefresh;

  const newRefresh = axios
    .post<{ accessToken: string }>(`${API_BASE_URL}/auth/refresh`, null, {
      withCredentials: true,
      ...transport,
    })
    .then((response) => response.data.accessToken)
    .finally(() => {
      refreshInFlight = null;
    });

  refreshInFlight = newRefresh;
  // Returning `newRefresh` (a const the .finally() closure never touches)
  // rather than re-reading `refreshInFlight` here — the outer `let` is
  // reassigned inside that closure, so TypeScript can't narrow it back to
  // non-null at this point even though we just set it on the line above.
  return newRefresh;
}

apiClient.interceptors.response.use(
  (response) => response,
  async (error) => {
    const originalRequest = error.config as
      | (InternalAxiosRequestConfig & { _retriedAfterRefresh?: boolean })
      | undefined;

    const isUnauthorized = error.response?.status === 401;
    const isRefreshCall = originalRequest?.url?.includes('/auth/refresh');
    const alreadyRetried = originalRequest?._retriedAfterRefresh;

    if (!isUnauthorized || isRefreshCall || alreadyRetried || !originalRequest) {
      return Promise.reject(error);
    }

    try {
      const newToken = await refreshAccessToken();
      authConfig?.onTokenRefreshed(newToken);
      originalRequest._retriedAfterRefresh = true;
      originalRequest.headers.set('Authorization', `Bearer ${newToken}`);
      return apiClient(originalRequest);
    } catch (refreshError) {
      authConfig?.onAuthFailure();
      return Promise.reject(refreshError);
    }
  },
);
