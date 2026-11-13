import { describe, expect, it } from 'vitest';
import { isDoctor, isPatient } from '@/shared/utils/guards';
import type { AuthenticatedUser } from '@/shared/types/api';

const doctorUser: AuthenticatedUser = {
  id: '1',
  role: 'Doctor',
  fullName: 'Dr. Sara El-Mansour',
  email: 'sara@example.com',
  preferredLanguage: 'en',
  doctorId: 'doctor-1',
};

const patientUser: AuthenticatedUser = {
  id: '2',
  role: 'Patient',
  fullName: 'Ahmed Mostafa Kamal',
  email: 'ahmed@example.com',
  preferredLanguage: 'ar',
  patientId: 'patient-1',
};

describe('isDoctor / isPatient', () => {
  it('accepts a full AuthenticatedUser object', () => {
    expect(isDoctor(doctorUser)).toBe(true);
    expect(isPatient(doctorUser)).toBe(false);
    expect(isPatient(patientUser)).toBe(true);
    expect(isDoctor(patientUser)).toBe(false);
  });

  it('accepts a bare role string', () => {
    expect(isDoctor('Doctor')).toBe(true);
    expect(isPatient('Patient')).toBe(true);
    expect(isDoctor('Patient')).toBe(false);
  });

  it('returns false for null/undefined (logged-out state)', () => {
    expect(isDoctor(null)).toBe(false);
    expect(isPatient(undefined)).toBe(false);
  });
});
