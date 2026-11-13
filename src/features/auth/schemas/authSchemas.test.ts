import { describe, expect, it } from 'vitest';
import {
  doctorSignupSchema,
  loginSchema,
  patientSignupSchema,
} from '@/features/auth/schemas/authSchemas';

const validPatient = {
  fullName: 'Ahmed Mostafa Kamal',
  phone: '01012345678',
  email: 'ahmed@example.com',
  age: 58,
  gender: 'male' as const,
  nationalId: '29001011234567', // 14 digits
  diagnosis: 'Prior myocardial infarction',
  medications: 'Bisoprolol 5mg',
  assignedDoctorName: '',
  deviceSerial: '',
  password: 'password123',
  confirmPassword: 'password123',
  consent: true as const,
};

describe('patientSignupSchema', () => {
  it('accepts a fully valid patient payload', () => {
    const result = patientSignupSchema.safeParse(validPatient);
    expect(result.success).toBe(true);
  });

  it('rejects a national ID that is not exactly 14 digits', () => {
    const result = patientSignupSchema.safeParse({ ...validPatient, nationalId: '123' });
    expect(result.success).toBe(false);
    if (!result.success) {
      expect(result.error.issues.some((issue) => issue.path.includes('nationalId'))).toBe(true);
    }
  });

  it('rejects mismatched password/confirmPassword', () => {
    const result = patientSignupSchema.safeParse({
      ...validPatient,
      confirmPassword: 'somethingElse',
    });
    expect(result.success).toBe(false);
    if (!result.success) {
      expect(result.error.issues[0]?.path).toContain('confirmPassword');
    }
  });

  it('rejects a password shorter than 8 characters', () => {
    const result = patientSignupSchema.safeParse({
      ...validPatient,
      password: 'short',
      confirmPassword: 'short',
    });
    expect(result.success).toBe(false);
  });

  it('rejects when consent is not explicitly true', () => {
    const result = patientSignupSchema.safeParse({ ...validPatient, consent: false });
    expect(result.success).toBe(false);
  });

  it('allows medications/assignedDoctorName/deviceSerial to be omitted', () => {
    const { medications, assignedDoctorName, deviceSerial, ...rest } = validPatient;
    const result = patientSignupSchema.safeParse(rest);
    expect(result.success).toBe(true);
  });
});

describe('doctorSignupSchema', () => {
  const validDoctor = {
    fullName: 'Dr. Sara El-Mansour',
    phone: '01098765432',
    email: 'sara@example.com',
    specialty: 'Cardiology Consultant',
    licenseNumber: 'EG-12345',
    hospital: 'Cairo University Hospitals',
    department: 'CCU',
    password: 'password123',
    confirmPassword: 'password123',
    consent: true as const,
  };

  it('accepts a fully valid doctor payload', () => {
    expect(doctorSignupSchema.safeParse(validDoctor).success).toBe(true);
  });

  it('requires licenseNumber', () => {
    const result = doctorSignupSchema.safeParse({ ...validDoctor, licenseNumber: '' });
    expect(result.success).toBe(false);
  });
});

describe('loginSchema', () => {
  it('accepts a valid email/password pair', () => {
    expect(loginSchema.safeParse({ email: 'a@b.com', password: 'x' }).success).toBe(true);
  });

  it('rejects an invalid email', () => {
    expect(loginSchema.safeParse({ email: 'not-an-email', password: 'x' }).success).toBe(false);
  });
});
