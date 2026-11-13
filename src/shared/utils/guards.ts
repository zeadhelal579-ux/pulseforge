import type { AuthenticatedUser, UserRole } from '@/shared/types/api';

/** True when the given user (or role string) is a Doctor. Accepts either so
 *  call sites don't have to unwrap `user.role` everywhere. */
export function isDoctor(subject: AuthenticatedUser | UserRole | null | undefined): boolean {
  const role = typeof subject === 'string' ? subject : subject?.role;
  return role === 'Doctor';
}

/** True when the given user (or role string) is a Patient. */
export function isPatient(subject: AuthenticatedUser | UserRole | null | undefined): boolean {
  const role = typeof subject === 'string' ? subject : subject?.role;
  return role === 'Patient';
}
