/** Which panel of the auth page is showing — purely a UI concern. */
export type AuthTab = 'signup' | 'login';

/**
 * Lower-case role used by the signup role toggle and form schemas. Deliberately
 * distinct from the backend's `UserRole` ('Patient' | 'Doctor', shared/types/api.ts)
 * so the two are never accidentally interchanged — this one maps to a segment
 * of the signup form, that one maps to the authenticated session's role claim.
 */
export type SignupRole = 'patient' | 'doctor';
