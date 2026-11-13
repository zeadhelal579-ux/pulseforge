import { z } from 'zod';

/**
 * Error messages are translation KEYS ("auth.errors.required"), not literal
 * English strings — LoginForm/SignupForm run each error through `t()` before
 * displaying it, so the same schema drives validation in both languages.
 */
const requiredString = (max = 255) =>
  z.string().trim().min(1, 'auth.errors.required').max(max);

const nationalIdSchema = z
  .string()
  .trim()
  .regex(/^\d{14}$/, 'auth.errors.nationalId');

const passwordSchema = z.string().min(8, 'auth.errors.passwordLength');

export const loginSchema = z.object({
  email: requiredString().email('auth.errors.email'),
  password: requiredString(),
});
export type LoginFormValues = z.infer<typeof loginSchema>;

export const patientSignupSchema = z
  .object({
    fullName: requiredString(120),
    phone: requiredString(20),
    email: requiredString().email('auth.errors.email'),
    age: z.coerce.number().int().min(0).max(120),
    gender: z.enum(['male', 'female'], { message: 'auth.errors.required' }),
    nationalId: nationalIdSchema,
    diagnosis: requiredString(2000),
    medications: z.string().trim().max(2000).optional().or(z.literal('')),
    assignedDoctorName: z.string().trim().max(120).optional().or(z.literal('')),
    deviceSerial: z.string().trim().max(60).optional().or(z.literal('')),
    password: passwordSchema,
    confirmPassword: z.string(),
    consent: z.literal(true, {
      errorMap: () => ({ message: 'auth.errors.consent' }),
    }),
  })
  .refine((data) => data.password === data.confirmPassword, {
    path: ['confirmPassword'],
    message: 'auth.errors.passwordMatch',
  });
export type PatientSignupFormValues = z.infer<typeof patientSignupSchema>;

export const doctorSignupSchema = z
  .object({
    fullName: requiredString(120),
    phone: requiredString(20),
    email: requiredString().email('auth.errors.email'),
    specialty: requiredString(120),
    licenseNumber: requiredString(60),
    hospital: requiredString(160),
    department: z.string().trim().max(120).optional().or(z.literal('')),
    password: passwordSchema,
    confirmPassword: z.string(),
    consent: z.literal(true, {
      errorMap: () => ({ message: 'auth.errors.consent' }),
    }),
  })
  .refine((data) => data.password === data.confirmPassword, {
    path: ['confirmPassword'],
    message: 'auth.errors.passwordMatch',
  });
export type DoctorSignupFormValues = z.infer<typeof doctorSignupSchema>;
