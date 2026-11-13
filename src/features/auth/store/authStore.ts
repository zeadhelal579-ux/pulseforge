import { create } from 'zustand';
import type { AuthenticatedUser } from '@/shared/types/api';

export type AuthStatus = 'idle' | 'authenticating' | 'authenticated' | 'unauthenticated';

interface AuthState {
  user: AuthenticatedUser | null;
  accessToken: string | null;
  status: AuthStatus;
  setSession: (accessToken: string, user: AuthenticatedUser) => void;
  /** Updates just the token (used after a silent 401→refresh cycle) without
   *  touching the already-known user object. */
  setAccessToken: (accessToken: string) => void;
  clearSession: () => void;
}

/**
 * Deliberately NOT wrapped in zustand's `persist` middleware. Both the access
 * token and the user object must not survive a hard refresh in storage — on
 * boot, AuthProvider re-establishes the session via the httpOnly refresh
 * cookie instead (see app/providers/AuthProvider.tsx).
 */
export const useAuthStore = create<AuthState>((set) => ({
  user: null,
  accessToken: null,
  status: 'idle',
  setSession: (accessToken, user) => set({ accessToken, user, status: 'authenticated' }),
  setAccessToken: (accessToken) => set({ accessToken }),
  clearSession: () => set({ accessToken: null, user: null, status: 'unauthenticated' }),
}));
