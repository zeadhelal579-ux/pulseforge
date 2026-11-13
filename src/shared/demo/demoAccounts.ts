/**
 * File: src/shared/demo/demoAccounts.ts
 * Layer: shared/demo (in-browser demo backend)
 * Purpose: The public list of one-click demo logins shown on the login page ("Demo accounts").
 * Logic: Emails/password are deliberately public: this is a sandbox with fictional data. Labels live in the
 *        locale files (`demo.accounts.<id>`), so this module stays language-agnostic.
 * Implementation: a readonly array; demoSeed.ts reads the same emails so UI and seed data cannot disagree.
 */
import { DEMO_PASSWORD } from '@/shared/demo/demoConstants';
import type { RiskStatusLevel, UserRole } from '@/shared/types/api';

export type DemoAccountId = 'doctor' | 'patientStable' | 'patientCaution' | 'patientCritical';

export interface DemoAccount {
  id: DemoAccountId;
  role: UserRole;
  email: string;
  password: string;
  /** Status the patient is simulated in (undefined for the doctor). */
  status?: RiskStatusLevel;
}

export const DEMO_ACCOUNTS: readonly DemoAccount[] = [
  { id: 'doctor', role: 'Doctor', email: 'doctor@cardiosense.demo', password: DEMO_PASSWORD },
  {
    id: 'patientStable',
    role: 'Patient',
    email: 'patient@cardiosense.demo',
    password: DEMO_PASSWORD,
    status: 'Stable',
  },
  {
    id: 'patientCaution',
    role: 'Patient',
    email: 'patient.caution@cardiosense.demo',
    password: DEMO_PASSWORD,
    status: 'Caution',
  },
  {
    id: 'patientCritical',
    role: 'Patient',
    email: 'patient.critical@cardiosense.demo',
    password: DEMO_PASSWORD,
    status: 'Critical',
  },
];
