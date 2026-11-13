/**
 * File: src/shared/demo/demoSeed.ts
 * Layer: shared/demo (in-browser demo backend)
 * Purpose: Builds the initial demo database: one demo doctor, a ward of fictional patients in different
 *          conditions, their devices, and the logins offered on the login page.
 * Logic:
 *  - Everything is fictional and obviously fake (phone numbers 0100000xxxx, national IDs 2900000000xxxx).
 *  - Three patients double as one-click logins (stable / caution / critical); the rest fill the doctor's list
 *    so search, filters and pagination have something to work on. One patient is intentionally NOT linked to
 *    the doctor so "Add New Patient" can be demonstrated, and one device is offline so the ward statistics
 *    show a realistic "connected devices" ratio.
 *  - Time-varying data (vitals, alerts, history) is not seeded; demoReadings.ts / demoModel.ts derive it.
 * Implementation: a compact data table expanded into the DemoDatabase shape.
 */
import { DEMO_ACCOUNTS, type DemoAccountId } from '@/shared/demo/demoAccounts';
import { DEMO_DB_VERSION, DEMO_UNASSIGNED_NATIONAL_ID } from '@/shared/demo/demoConstants';
import type {
  DemoAssignment,
  DemoDatabase,
  DemoDeviceRecord,
  DemoDoctorRecord,
  DemoPatientRecord,
  DemoScenario,
  DemoUserRecord,
} from '@/shared/demo/demoTypes';
import type { ConnectionMode, Gender, MedicalHistoryEntry } from '@/shared/types/api';

/** One row of the patient table below. */
interface SeedPatient {
  key: string;
  name: string;
  age: number;
  gender: Gender;
  diagnoses: string[];
  medications: string[];
  scenario: Exclude<DemoScenario, 'idle'>;
  alertKind: 'Clinical' | 'Technical' | null;
  heartRate: number;
  spo2: number;
  riskScore: number;
  confidence: number;
  battery: number;
  mode: ConnectionMode;
  signalDbm: number;
  bed: string;
  offlineMinutes?: number;
  accountId?: DemoAccountId;
  /** False for the one patient the doctor can add later via "Add New Patient". */
  linkedToDoctor?: boolean;
  nationalId?: string;
}

const DOCTOR_ID = 'demo-doctor-1';
const DOCTOR_USER_ID = 'demo-user-doctor';

// The table below is deliberately kept compact (one patient per block) so it stays readable.
// prettier-ignore
const PATIENT_SEEDS: SeedPatient[] = [
  {
    key: 'ahmed', name: 'Ahmed Mostafa Kamal', age: 58, gender: 'male',
    diagnoses: ['Prior myocardial infarction', 'Hypertension'],
    medications: ['Bisoprolol 5 mg', 'Aspirin 100 mg', 'Atorvastatin 40 mg'],
    scenario: 'stable', alertKind: null, heartRate: 74, spo2: 98, riskScore: 18, confidence: 0.94,
    battery: 82, mode: 'WiFi', signalDbm: -58, bed: 'CCU-04', accountId: 'patientStable',
  },
  {
    key: 'fatima', name: 'Fatima Hassan Ibrahim', age: 63, gender: 'female',
    diagnoses: ['Paroxysmal atrial fibrillation', 'Hypertension'],
    medications: ['Apixaban 5 mg', 'Bisoprolol 2.5 mg'],
    scenario: 'caution', alertKind: 'Clinical', heartRate: 98, spo2: 94, riskScore: 52, confidence: 0.88,
    battery: 64, mode: 'WiFi', signalDbm: -63, bed: 'W-204', accountId: 'patientCaution',
  },
  {
    key: 'mahmoud', name: 'Mahmoud Abdelrahman', age: 67, gender: 'male',
    diagnoses: ['NSTEMI follow-up', 'Type 2 diabetes'],
    medications: ['Ticagrelor 90 mg', 'Aspirin 75 mg', 'Metformin 1000 mg'],
    scenario: 'critical', alertKind: 'Clinical', heartRate: 118, spo2: 90, riskScore: 86, confidence: 0.91,
    battery: 41, mode: 'Bluetooth', signalDbm: -71, bed: 'CCU-07', accountId: 'patientCritical',
  },
  {
    key: 'souad', name: 'Souad Farouk', age: 71, gender: 'female',
    diagnoses: ['Heart failure (NYHA II)', 'Hypertension'],
    medications: ['Furosemide 40 mg', 'Carvedilol 6.25 mg', 'Ramipril 5 mg'],
    scenario: 'stable', alertKind: null, heartRate: 70, spo2: 97, riskScore: 22, confidence: 0.92,
    battery: 77, mode: 'WiFi', signalDbm: -55, bed: 'W-112',
  },
  {
    key: 'youssef', name: 'Youssef Adel Samir', age: 49, gender: 'male',
    diagnoses: ['Post-PCI follow-up (LAD stent)'],
    medications: ['Clopidogrel 75 mg', 'Aspirin 100 mg', 'Rosuvastatin 20 mg'],
    scenario: 'stable', alertKind: null, heartRate: 68, spo2: 99, riskScore: 12, confidence: 0.96,
    battery: 91, mode: 'WiFi', signalDbm: -52, bed: 'W-118',
  },
  {
    key: 'mona', name: 'Mona Ahmed Saleh', age: 55, gender: 'female',
    diagnoses: ['Stable angina', 'Dyslipidemia'],
    medications: ['Isosorbide mononitrate 30 mg', 'Atorvastatin 20 mg'],
    scenario: 'stable', alertKind: null, heartRate: 72, spo2: 98, riskScore: 16, confidence: 0.95,
    battery: 58, mode: 'WiFi', signalDbm: -60, bed: 'W-121',
  },
  {
    key: 'karim', name: 'Karim Tarek Nabil', age: 44, gender: 'male',
    diagnoses: ['Hypertrophic cardiomyopathy'],
    medications: ['Metoprolol 50 mg'],
    scenario: 'stable', alertKind: null, heartRate: 64, spo2: 99, riskScore: 20, confidence: 0.93,
    battery: 96, mode: 'WiFi', signalDbm: -50, bed: 'W-130',
  },
  {
    key: 'laila', name: 'Laila Mahmoud Fouad', age: 61, gender: 'female',
    diagnoses: ['Mitral valve repair follow-up'],
    medications: ['Warfarin 3 mg', 'Bisoprolol 2.5 mg'],
    scenario: 'stable', alertKind: null, heartRate: 76, spo2: 97, riskScore: 19, confidence: 0.92,
    battery: 35, mode: 'Bluetooth', signalDbm: -74, bed: 'W-133', offlineMinutes: 38,
  },
  {
    key: 'omar', name: 'Omar Khaled Hegazy', age: 52, gender: 'male',
    diagnoses: ['Prior CABG', 'Type 2 diabetes'],
    medications: ['Aspirin 100 mg', 'Metformin 500 mg', 'Atorvastatin 40 mg'],
    scenario: 'stable', alertKind: null, heartRate: 78, spo2: 97, riskScore: 24, confidence: 0.9,
    battery: 69, mode: 'WiFi', signalDbm: -57, bed: 'W-140',
  },
  {
    key: 'nadia', name: 'Nadia Ibrahim Shafik', age: 69, gender: 'female',
    diagnoses: ['Hypertensive heart disease'],
    medications: ['Amlodipine 5 mg', 'Valsartan 80 mg'],
    scenario: 'caution', alertKind: 'Technical', heartRate: 80, spo2: 96, riskScore: 34, confidence: 0.7,
    battery: 54, mode: 'WiFi', signalDbm: -79, bed: 'W-150',
  },
  {
    key: 'hassan', name: 'Hassan Mohamed Elsayed', age: 73, gender: 'male',
    diagnoses: ['Ischemic cardiomyopathy (EF 30%)'],
    medications: ['Sacubitril/valsartan 49/51 mg', 'Carvedilol 12.5 mg', 'Spironolactone 25 mg'],
    scenario: 'critical', alertKind: 'Clinical', heartRate: 112, spo2: 91, riskScore: 81, confidence: 0.89,
    battery: 48, mode: 'WiFi', signalDbm: -66, bed: 'CCU-02',
  },
  {
    key: 'rania', name: 'Rania Sameh Abdelfattah', age: 57, gender: 'female',
    diagnoses: ['Hypertension', 'Palpitations under evaluation'],
    medications: ['Telmisartan 40 mg'],
    scenario: 'caution', alertKind: 'Clinical', heartRate: 102, spo2: 96, riskScore: 47, confidence: 0.86,
    battery: 73, mode: 'WiFi', signalDbm: -61, bed: 'W-160',
  },
  {
    key: 'walid', name: 'Walid Samir Mansour', age: 60, gender: 'male',
    diagnoses: ['Stable angina'],
    medications: ['Aspirin 100 mg'],
    scenario: 'stable', alertKind: null, heartRate: 71, spo2: 98, riskScore: 15, confidence: 0.95,
    battery: 88, mode: 'WiFi', signalDbm: -54, bed: 'W-170',
    linkedToDoctor: false, nationalId: DEMO_UNASSIGNED_NATIONAL_ID,
  },
];

/** Spare devices in "inventory" that nobody has paired yet (handy for the device-pairing demo). */
const SPARE_DEVICE_SERIALS = ['CS-95001-EG', 'CS-95002-EG', 'CS-95003-EG'];

/** Email of a demo account (throws if the id is unknown: that would be a programming error). */
function accountEmail(id: DemoAccountId): string {
  const account = DEMO_ACCOUNTS.find((candidate) => candidate.id === id);
  if (!account) throw new Error(`Unknown demo account: ${id}`);
  return account.email;
}

/** Expands diagnosis / medication lists into medical-history rows (one row per item, like the real backend). */
function buildHistory(
  patientId: string,
  seed: SeedPatient,
  recordedAt: string,
): MedicalHistoryEntry[] {
  const diagnoses = seed.diagnoses.map<MedicalHistoryEntry>((description, index) => ({
    id: `${patientId}-dx-${index + 1}`,
    entryType: 'Diagnosis',
    description,
    recordedAt,
  }));
  const medications = seed.medications.map<MedicalHistoryEntry>((description, index) => ({
    id: `${patientId}-rx-${index + 1}`,
    entryType: 'Medication',
    description,
    recordedAt,
  }));
  return [...diagnoses, ...medications];
}

/** Creates the complete initial database. `nowMs` only stamps creation times. */
export function createSeedDatabase(nowMs: number): DemoDatabase {
  const nowIso = new Date(nowMs).toISOString();

  const doctor: DemoDoctorRecord = {
    id: DOCTOR_ID,
    userId: DOCTOR_USER_ID,
    fullName: 'Dr. Sara El-Mansour',
    specialty: 'Cardiology Consultant',
    licenseNumber: 'DEMO-0001',
    hospital: 'Cairo University Hospitals',
    department: 'Cardiac Care Unit (CCU)',
    phone: '01000000000',
    email: accountEmail('doctor'),
  };

  const users: DemoUserRecord[] = [
    {
      id: DOCTOR_USER_ID,
      email: doctor.email,
      role: 'Doctor',
      profileId: DOCTOR_ID,
      preferredLanguage: 'ar',
      credential: { kind: 'demo' },
    },
  ];
  const patients: DemoPatientRecord[] = [];
  const assignments: DemoAssignment[] = [];
  const devices: DemoDeviceRecord[] = [];

  PATIENT_SEEDS.forEach((seed, index) => {
    const number = index + 1;
    const patientId = `demo-patient-${number}`;
    const email = seed.accountId ? accountEmail(seed.accountId) : `${seed.key}@cardiosense.demo`;
    const userId = seed.accountId ? `demo-user-${seed.key}` : null;
    const serialNumber = `CS-${94280 + number}-EG`;

    if (userId) {
      users.push({
        id: userId,
        email,
        role: 'Patient',
        profileId: patientId,
        preferredLanguage: 'ar',
        credential: { kind: 'demo' },
      });
    }

    patients.push({
      id: patientId,
      userId,
      fileNumber: `CS-${8900 + number}`,
      fullName: seed.name,
      age: seed.age,
      gender: seed.gender,
      nationalId: seed.nationalId ?? `2900000000${String(number).padStart(4, '0')}`,
      phone: `0100000${String(number).padStart(4, '0')}`,
      email,
      diagnosis: seed.diagnoses.join(', '),
      medications: seed.medications.join(', '),
      medicalHistory: buildHistory(patientId, seed, nowIso),
      scenario: seed.scenario,
      alertKind: seed.alertKind,
      baseline: {
        heartRate: seed.heartRate,
        spo2: seed.spo2,
        riskScore: seed.riskScore,
        confidence: seed.confidence,
      },
      bedOrRoom: seed.bed,
      offlineMinutes: seed.offlineMinutes ?? 0,
      dataSinceMs: null,
      createdAt: nowIso,
    });

    devices.push({
      serialNumber,
      patientId,
      pairedAt: nowIso,
      firmwareVersion: '1.4.2',
      batteryLevel: seed.battery,
      connectionMode: seed.mode,
      signalStrengthDbm: seed.signalDbm,
    });

    if (seed.linkedToDoctor !== false) {
      assignments.push({ doctorId: DOCTOR_ID, patientId, assignedAt: nowIso, isPrimary: true });
    }
  });

  for (const serialNumber of SPARE_DEVICE_SERIALS) {
    devices.push({
      serialNumber,
      patientId: null,
      pairedAt: null,
      firmwareVersion: '1.4.2',
      batteryLevel: 100,
      connectionMode: 'WiFi',
      signalStrengthDbm: -55,
    });
  }

  return {
    version: DEMO_DB_VERSION,
    seededAt: nowIso,
    users,
    patients,
    doctors: [doctor],
    assignments,
    devices,
    manualAlerts: [],
  };
}
