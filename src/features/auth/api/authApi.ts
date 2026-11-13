import { apiClient } from '@/shared/lib/apiClient';
import type {
  AuthResponse,
  DoctorSignupRequest,
  LoginRequest,
  PatientSignupRequest,
  RefreshResponse,
} from '@/shared/types/api';

/** POST /api/auth/login — backend-database-architecture.md §5. */
export async function login(payload: LoginRequest): Promise<AuthResponse> {
  const { data } = await apiClient.post<AuthResponse>('/auth/login', payload);
  return data;
}

/** POST /api/auth/signup/patient */
export async function signupPatient(payload: PatientSignupRequest): Promise<AuthResponse> {
  const { data } = await apiClient.post<AuthResponse>('/auth/signup/patient', payload);
  return data;
}

/** POST /api/auth/signup/doctor */
export async function signupDoctor(payload: DoctorSignupRequest): Promise<AuthResponse> {
  const { data } = await apiClient.post<AuthResponse>('/auth/signup/doctor', payload);
  return data;
}

/**
 * POST /api/auth/refresh — relies entirely on the httpOnly refresh cookie the
 * browser attaches automatically (see apiClient's `withCredentials: true`),
 * so this call takes no body.
 */
export async function refreshSession(): Promise<RefreshResponse> {
  const { data } = await apiClient.post<RefreshResponse>('/auth/refresh');
  return data;
}

/**
 * POST /api/auth/logout — ASSUMPTION / GAP: not listed in backend-database-
 * architecture.md §5's endpoint table. Without a server-side call, "logout"
 * can only clear the in-memory session on this tab — the refresh token
 * cookie (and its hashed row in RefreshTokens, §6) would remain valid until
 * it naturally expires. This endpoint should revoke it server-side; add it
 * to CardioSense.Api/Endpoints/AuthEndpoints.cs.
 */
export async function logout(): Promise<void> {
  await apiClient.post('/auth/logout');
}
