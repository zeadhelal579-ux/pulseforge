import { beforeEach, describe, expect, it } from 'vitest';
import { createAccessToken } from '@/shared/demo/demoAuth';
import { DEMO_UNASSIGNED_NATIONAL_ID } from '@/shared/demo/demoConstants';
import {
  CAUTION_PATIENT_EMAIL,
  CRITICAL_PATIENT_EMAIL,
  DOCTOR_EMAIL,
  FIXED_NOW,
  STABLE_PATIENT_EMAIL,
  call,
  freshDemo,
  loginAs,
  setUiLanguage,
} from '@/test/demoTestUtils';
import type {
  AiReport,
  Alert,
  PaginatedResponse,
  PatientListItem,
  PatientProfile,
  VitalsSample,
  WardStats,
} from '@/shared/types/api';

const NEW_PATIENT = {
  fullName: 'Test Patient',
  phone: '01012345678',
  email: 'New.Patient@example.com',
  age: 41,
  gender: 'female',
  nationalId: '29512345678901',
  diagnosis: 'Palpitations',
  medications: 'None',
  password: 'Secret123',
  consent: true,
};

describe('demo backend: authentication', () => {
  beforeEach(freshDemo);

  it('signs in every seeded account and issues a JWT-shaped token with the real claims', async () => {
    const doctor = await loginAs(DOCTOR_EMAIL);
    expect(doctor.user.role).toBe('Doctor');
    expect(doctor.user.doctorId).toBeTruthy();
    expect(doctor.accessToken.split('.')).toHaveLength(3);

    for (const email of [STABLE_PATIENT_EMAIL, CAUTION_PATIENT_EMAIL, CRITICAL_PATIENT_EMAIL]) {
      const patient = await loginAs(email);
      expect(patient.user.role).toBe('Patient');
      expect(patient.user.patientId).toBeTruthy();
    }
  });

  it('rejects a wrong password and an unknown email with the same error', async () => {
    const wrongPassword = await call('POST', '/auth/login', {
      body: { email: DOCTOR_EMAIL, password: 'nope-nope' },
    });
    const unknownEmail = await call('POST', '/auth/login', {
      body: { email: 'ghost@example.com', password: 'Demo@1234' },
    });
    expect(wrongPassword.status).toBe(401);
    expect(unknownEmail.status).toBe(401);
    expect(wrongPassword.data).toEqual(unknownEmail.data);
  });

  it('keeps the visitor language instead of snapping back to a stored preference', async () => {
    setUiLanguage('en');
    expect((await loginAs(DOCTOR_EMAIL)).user.preferredLanguage).toBe('en');
    setUiLanguage('ar');
    expect((await loginAs(DOCTOR_EMAIL)).user.preferredLanguage).toBe('ar');
  });

  it('requires a valid, unexpired access token on protected routes', async () => {
    expect((await call('GET', '/patients/me')).status).toBe(401);
    expect((await call('GET', '/patients/me', { token: 'garbage.token.value' })).status).toBe(401);

    const { accessToken } = await loginAs(STABLE_PATIENT_EMAIL);
    expect((await call('GET', '/patients/me', { token: accessToken })).status).toBe(200);

    const sixteenMinutesLater = FIXED_NOW + 16 * 60 * 1000;
    const expired = await call('GET', '/patients/me', { token: accessToken, nowMs: sixteenMinutesLater });
    expect(expired.status).toBe(401);
  });

  it('refreshes while a session exists and stops after logout', async () => {
    expect((await call('POST', '/auth/refresh')).status).toBe(401);
    await loginAs(STABLE_PATIENT_EMAIL);

    const refreshed = await call('POST', '/auth/refresh');
    expect(refreshed.status).toBe(200);
    expect((refreshed.data as { accessToken: string }).accessToken.split('.')).toHaveLength(3);

    expect((await call('POST', '/auth/logout')).status).toBe(204);
    expect((await call('POST', '/auth/refresh')).status).toBe(401);
  });
});

describe('demo backend: sign-up', () => {
  beforeEach(freshDemo);

  it('creates a patient who can then sign in, with no readings until a device is paired', async () => {
    const created = await call('POST', '/auth/signup/patient', { body: NEW_PATIENT });
    expect(created.status).toBe(201);

    // The email is normalised to lower case and the stored password is never the plain text.
    const { accessToken } = await loginAs('new.patient@example.com', 'Secret123');
    const raw = JSON.stringify(localStorage.getItem('cardiosense.demo.db.v1'));
    expect(raw.includes('Secret123')).toBe(false);

    const profile = (await call('GET', '/patients/me', { token: accessToken })).data as PatientProfile;
    expect(profile.fullName).toBe('Test Patient');
    expect(profile.latestVitals).toBeNull();
    expect(profile.latestRiskAssessment).toBeNull();
    expect(profile.hourlyStatusTimeline).toHaveLength(0);
    expect(profile.device).toBeNull();
    expect(profile.medicalHistory.map((entry) => entry.entryType)).toEqual(['Diagnosis', 'Medication']);

    const history = (await call('GET', '/patients/me/vitals-history', { token: accessToken })).data;
    expect(history).toEqual([]);
    expect((await call('GET', '/patients/me/ai-report', { token: accessToken })).status).toBe(404);
  });

  it('rejects duplicates and invalid fields', async () => {
    expect((await call('POST', '/auth/signup/patient', { body: NEW_PATIENT })).status).toBe(201);

    const sameEmail = await call('POST', '/auth/signup/patient', {
      body: { ...NEW_PATIENT, nationalId: '29512345678902' },
    });
    expect(sameEmail.status).toBe(409);

    const sameNationalId = await call('POST', '/auth/signup/patient', {
      body: { ...NEW_PATIENT, email: 'other@example.com' },
    });
    expect(sameNationalId.status).toBe(409);

    const invalid = await call('POST', '/auth/signup/patient', {
      body: { ...NEW_PATIENT, email: 'x@example.com', nationalId: '123', password: 'short' },
    });
    expect(invalid.status).toBe(400);
    const errors = (invalid.data as { errors: Record<string, string[]> }).errors;
    expect(Object.keys(errors).sort()).toEqual(['nationalId', 'password']);
  });

  it('links the doctor only when exactly one doctor has that name', async () => {
    await call('POST', '/auth/signup/patient', {
      body: { ...NEW_PATIENT, assignedDoctorName: 'sara el-mansour' },
    });
    const { accessToken } = await loginAs('new.patient@example.com', 'Secret123');
    const profile = (await call('GET', '/patients/me', { token: accessToken })).data as PatientProfile;
    expect(profile.assignedDoctor?.fullName).toBe('Dr. Sara El-Mansour');

    await call('POST', '/auth/signup/patient', {
      body: { ...NEW_PATIENT, email: 'b@example.com', nationalId: '29512345678903', assignedDoctorName: 'Nobody' },
    });
    const other = await loginAs('b@example.com', 'Secret123');
    const otherProfile = (await call('GET', '/patients/me', { token: other.accessToken })).data as PatientProfile;
    expect(otherProfile.assignedDoctor).toBeNull();
  });

  it('creates a doctor who can sign in, and rejects a duplicate license number', async () => {
    const body = {
      fullName: 'Dr. New Doctor',
      phone: '01011111111',
      email: 'new.doctor@example.com',
      password: 'Secret123',
      specialty: 'Cardiology',
      licenseNumber: 'LIC-777',
      hospital: 'Demo Hospital',
    };
    expect((await call('POST', '/auth/signup/doctor', { body })).status).toBe(201);
    const { user } = await loginAs('new.doctor@example.com', 'Secret123');
    expect(user.role).toBe('Doctor');

    const duplicate = await call('POST', '/auth/signup/doctor', {
      body: { ...body, email: 'another@example.com', licenseNumber: 'lic-777' },
    });
    expect(duplicate.status).toBe(409);
  });
});

describe('demo backend: access rules', () => {
  beforeEach(freshDemo);

  it('lets a patient read only their own record', async () => {
    const { accessToken } = await loginAs(STABLE_PATIENT_EMAIL);
    expect((await call('GET', '/patients/demo-patient-1', { token: accessToken })).status).toBe(200);
    expect((await call('GET', '/patients/demo-patient-2', { token: accessToken })).status).toBe(403);
    expect((await call('GET', '/patients/demo-patient-2/alerts', { token: accessToken })).status).toBe(403);
  });

  it('keeps the numeric risk output away from patients but gives it to doctors', async () => {
    const patient = await loginAs(CRITICAL_PATIENT_EMAIL);
    const doctor = await loginAs(DOCTOR_EMAIL);

    const asPatient = (await call('GET', '/patients/me', { token: patient.accessToken })).data as PatientProfile;
    expect(asPatient.latestRiskAssessment?.statusLevel).toBe('Critical');
    expect(asPatient.latestRiskAssessment?.riskScore).toBeUndefined();
    expect(asPatient.latestRiskAssessment?.confidenceScore).toBe(0);
    expect(asPatient.latestRiskAssessment?.flags).toEqual([]);

    const asDoctor = (await call('GET', '/patients/demo-patient-3', { token: doctor.accessToken })).data as PatientProfile;
    expect(typeof asDoctor.latestRiskAssessment?.riskScore).toBe('number');
    expect(asDoctor.latestRiskAssessment?.flags).toContain('possible_st_elevation');
  });

  it('gives doctors no personal profile and patients no doctor endpoints', async () => {
    const doctor = await loginAs(DOCTOR_EMAIL);
    const patient = await loginAs(STABLE_PATIENT_EMAIL);
    expect((await call('GET', '/patients/me', { token: doctor.accessToken })).status).toBe(403);
    expect((await call('GET', '/doctors/me', { token: patient.accessToken })).status).toBe(403);
    expect((await call('GET', '/doctors/me/patients', { token: patient.accessToken })).status).toBe(403);
    expect((await call('GET', '/doctors/me/ward-stats', { token: patient.accessToken })).status).toBe(403);
  });

  it('hides a patient from the doctor until the doctor adds them by national ID', async () => {
    const { accessToken } = await loginAs(DOCTOR_EMAIL);
    const walid = '/patients/demo-patient-13';
    expect((await call('GET', walid, { token: accessToken })).status).toBe(403);

    const unknown = await call('POST', '/doctors/me/patients', { token: accessToken, body: { identifier: '0' } });
    expect(unknown.status).toBe(404);

    const added = await call('POST', '/doctors/me/patients', {
      token: accessToken,
      body: { identifier: DEMO_UNASSIGNED_NATIONAL_ID },
    });
    expect(added.status).toBe(201);
    expect((await call('GET', walid, { token: accessToken })).status).toBe(200);

    const again = await call('POST', '/doctors/me/patients', {
      token: accessToken,
      body: { identifier: DEMO_UNASSIGNED_NATIONAL_ID },
    });
    expect(again.status).toBe(200); // idempotent
  });

  it('reserves clinical actions for doctors and validates them', async () => {
    const doctor = await loginAs(DOCTOR_EMAIL);
    const patient = await loginAs(STABLE_PATIENT_EMAIL);
    const path = '/patients/demo-patient-1';

    expect((await call('POST', `${path}/freeze-waveform`, { token: patient.accessToken })).status).toBe(403);
    expect((await call('POST', `${path}/freeze-waveform`, { token: doctor.accessToken })).status).toBe(204);

    const asPatient = await call('POST', `${path}/manual-events`, {
      token: patient.accessToken,
      body: { note: 'x', severity: 'Info' },
    });
    expect(asPatient.status).toBe(403);

    const badSeverity = await call('POST', `${path}/manual-events`, {
      token: doctor.accessToken,
      body: { note: 'Check on patient', severity: 'Dramatic' },
    });
    expect(badSeverity.status).toBe(400);

    const created = await call('POST', `${path}/manual-events`, {
      token: doctor.accessToken,
      body: { note: 'Chest discomfort reported by phone', severity: 'Critical' },
    });
    expect(created.status).toBe(201);

    const alerts = (await call('GET', `${path}/alerts`, { token: patient.accessToken })).data as Alert[];
    expect(alerts[0]?.message).toBe('Chest discomfort reported by phone');
    expect(alerts[0]?.severity).toBe('Critical');
    expect(alerts[0]?.type).toBe('Manual Event');
  });
});

describe('demo backend: doctor dashboard', () => {
  beforeEach(freshDemo);

  it('lists only this doctor\'s patients, most severe first, five per page', async () => {
    const { accessToken } = await loginAs(DOCTOR_EMAIL);
    const page1 = (await call('GET', '/doctors/me/patients', { token: accessToken })).data as PaginatedResponse<PatientListItem>;
    expect(page1.totalCount).toBe(12); // 13 seeded patients, one not linked to the doctor
    expect(page1.totalPages).toBe(3);
    expect(page1.items).toHaveLength(5);
    expect(page1.items.map((item) => item.statusLevel)).toEqual([
      'Critical',
      'Critical',
      'Caution',
      'Caution',
      'Caution',
    ]);
    expect(page1.items[0]?.fullName).toBe('Hassan Mohamed Elsayed');

    const page3 = (await call('GET', '/doctors/me/patients', { token: accessToken, query: { page: '3' } }))
      .data as PaginatedResponse<PatientListItem>;
    expect(page3.items).toHaveLength(2);
    expect(page3.items.every((item) => item.statusLevel === 'Stable')).toBe(true);

    const clamped = (await call('GET', '/doctors/me/patients', { token: accessToken, query: { page: '99' } }))
      .data as PaginatedResponse<PatientListItem>;
    expect(clamped.page).toBe(3);
  });

  it('filters by status and searches by name, file number or national ID', async () => {
    const { accessToken } = await loginAs(DOCTOR_EMAIL);
    const list = async (query: Record<string, string>) =>
      (await call('GET', '/doctors/me/patients', { token: accessToken, query })).data as PaginatedResponse<PatientListItem>;

    expect((await list({ status: 'critical' })).totalCount).toBe(2);
    expect((await list({ status: 'alert' })).totalCount).toBe(3);
    expect((await list({ status: 'stable' })).totalCount).toBe(7);
    // Name search is a case-insensitive "contains" match, so "ahmed" finds both Ahmed Mostafa Kamal and Mona Ahmed Saleh.
    expect((await list({ search: 'ahmed' })).totalCount).toBe(2);
    expect((await list({ search: 'MOSTAFA' })).items.map((item) => item.fullName)).toEqual(['Ahmed Mostafa Kamal']);
    expect((await list({ search: 'cs-8903' })).items[0]?.fullName).toBe('Mahmoud Abdelrahman');
    expect((await list({ search: '29000000000004' })).items[0]?.fullName).toBe('Souad Farouk');
    expect((await call('GET', '/doctors/me/patients', { token: accessToken, query: { status: 'bogus' } })).status).toBe(400);
  });

  it('computes ward statistics from this doctor\'s patients only', async () => {
    const { accessToken } = await loginAs(DOCTOR_EMAIL);
    const stats = (await call('GET', '/doctors/me/ward-stats', { token: accessToken })).data as WardStats;
    expect(stats.activePatients).toBe(12);
    expect(stats.criticalCasesCount).toBe(2);
    expect(stats.cautionCount).toBe(3);
    expect(stats.stableCount).toBe(7);
    expect(stats.stableCount + stats.cautionCount + stats.criticalCasesCount).toBe(stats.activePatients);
    expect(stats.totalDevices).toBe(12);
    expect(stats.connectedDevices).toBe(11); // one seeded device is offline
    expect(stats.deviceLinkRatePercent).toBe(92);
    expect(stats.signalAccuracyPercent).toBeGreaterThan(50);
  });

  it('localises server-generated text to the active UI language', async () => {
    const { accessToken } = await loginAs(DOCTOR_EMAIL);
    const firstLabel = async () =>
      ((await call('GET', '/doctors/me/patients', { token: accessToken })).data as PaginatedResponse<PatientListItem>)
        .items[0]?.statusLabel;

    setUiLanguage('en');
    expect(await firstLabel()).toBe('Critical Alert');
    setUiLanguage('ar');
    expect(await firstLabel()).toBe('تنبيه حرج');
  });

  it('updates the doctor profile partially and never touches the license number', async () => {
    const { accessToken } = await loginAs(DOCTOR_EMAIL);
    const updated = await call('PUT', '/doctors/me', {
      token: accessToken,
      body: { hospital: 'Ain Shams University Hospitals', licenseNumber: 'HACKED', preferredLanguage: 'en' },
    });
    const profile = updated.data as { hospital: string; licenseNumber: string };
    expect(profile.hospital).toBe('Ain Shams University Hospitals');
    expect(profile.licenseNumber).toBe('DEMO-0001');
  });
});

describe('demo backend: readings, history and report', () => {
  beforeEach(freshDemo);

  it('returns one real point per bucket for each history range', async () => {
    const { accessToken } = await loginAs(STABLE_PATIENT_EMAIL);
    const history = async (range: string) =>
      (await call('GET', '/patients/me/vitals-history', { token: accessToken, query: { range } })).data as VitalsSample[];

    expect(await history('24h')).toHaveLength(288);
    expect(await history('7d')).toHaveLength(168);
    expect(await history('30d')).toHaveLength(120);

    const day = await history('24h');
    expect(day.every((sample) => sample.heartRate >= 35 && sample.spo2 <= 100)).toBe(true);
    expect(Date.parse(day[0]?.timestamp ?? '')).toBeLessThan(Date.parse(day[day.length - 1]?.timestamp ?? ''));
    expect((await call('GET', '/patients/me/vitals-history', { token: accessToken, query: { range: '1y' } })).status).toBe(400);
  });

  it('builds a 24-bucket timeline whose current hour matches the live status', async () => {
    for (const [email, expected] of [
      [STABLE_PATIENT_EMAIL, 'Stable'],
      [CAUTION_PATIENT_EMAIL, 'Caution'],
      [CRITICAL_PATIENT_EMAIL, 'Critical'],
    ] as const) {
      const { accessToken } = await loginAs(email);
      const profile = (await call('GET', '/patients/me', { token: accessToken })).data as PatientProfile;
      expect(profile.hourlyStatusTimeline).toHaveLength(24);
      expect(profile.hourlyStatusTimeline.filter((bucket) => bucket.isCurrent)).toHaveLength(1);
      expect(profile.hourlyStatusTimeline[23]?.status).toBe(expected);
      expect(profile.latestRiskAssessment?.statusLevel).toBe(expected);
    }
  });

  it('is deterministic for the same patient and moment', async () => {
    const { accessToken } = await loginAs(STABLE_PATIENT_EMAIL);
    const first = (await call('GET', '/patients/me', { token: accessToken })).data as PatientProfile;
    const second = (await call('GET', '/patients/me', { token: accessToken })).data as PatientProfile;
    expect(first.latestVitals).toEqual(second.latestVitals);
  });

  it('writes a report from real period statistics and tailors it to the viewer', async () => {
    const patient = await loginAs(CRITICAL_PATIENT_EMAIL);
    const doctor = await loginAs(DOCTOR_EMAIL);

    const patientReport = (await call('GET', '/patients/me/ai-report', { token: patient.accessToken, query: { period: 'weekly' } }))
      .data as AiReport;
    const doctorReport = (await call('GET', '/patients/demo-patient-3/ai-report', { token: doctor.accessToken, query: { period: 'weekly' } }))
      .data as AiReport;

    expect(patientReport.period).toBe('weekly');
    expect(patientReport.averageHeartRate).toBeGreaterThan(100);
    expect(patientReport.alertsLoggedCount).toBeGreaterThan(0);
    expect(patientReport.narrativeSummary).toContain('simulated demo data');
    // Doctor-only model detail must not leak into the patient's text.
    expect(doctorReport.narrativeSummary).toContain('ST-segment');
    expect(patientReport.narrativeSummary).not.toContain('ST-segment');
    expect((await call('GET', '/patients/me/ai-report', { token: patient.accessToken, query: { period: 'yearly' } })).status).toBe(400);
  });

  it('marks the offline device in the list and serves older data for it', async () => {
    const { accessToken } = await loginAs(DOCTOR_EMAIL);
    const list = (await call('GET', '/doctors/me/patients', { token: accessToken, query: { search: 'laila' } }))
      .data as PaginatedResponse<PatientListItem>;
    const laila = list.items[0];
    expect(laila?.isLive).toBe(false);
    expect(FIXED_NOW - Date.parse(laila?.lastUpdatedAt ?? '')).toBe(38 * 60 * 1000);
  });
});

describe('demo backend: device pairing', () => {
  beforeEach(freshDemo);

  it('pairs a device for a new patient and starts producing readings', async () => {
    await call('POST', '/auth/signup/patient', { body: NEW_PATIENT });
    const { accessToken } = await loginAs('new.patient@example.com', 'Secret123');

    const paired = await call('POST', '/devices/pair', { token: accessToken, body: { serialNumber: 'cs-95001-eg' } });
    expect(paired.status).toBe(200);

    const profile = (await call('GET', '/patients/me', { token: accessToken })).data as PatientProfile;
    expect(profile.device?.serialNumber).toBe('CS-95001-EG');
    expect(profile.latestVitals).not.toBeNull();
    expect(profile.hourlyStatusTimeline).toHaveLength(1); // data exists only since the pairing hour

    const status = await call('GET', '/devices/CS-95001-EG/status', { token: accessToken });
    expect(status.status).toBe(200);
  });

  it('enforces one device per patient and one patient per device', async () => {
    await call('POST', '/auth/signup/patient', { body: NEW_PATIENT });
    const { accessToken } = await loginAs('new.patient@example.com', 'Secret123');

    const taken = await call('POST', '/devices/pair', { token: accessToken, body: { serialNumber: 'CS-94281-EG' } });
    expect(taken.status).toBe(409);

    const badFormat = await call('POST', '/devices/pair', { token: accessToken, body: { serialNumber: '!!' } });
    expect(badFormat.status).toBe(400);

    expect((await call('POST', '/devices/pair', { token: accessToken, body: { serialNumber: 'CS-95002-EG' } })).status).toBe(200);
    const second = await call('POST', '/devices/pair', { token: accessToken, body: { serialNumber: 'CS-95003-EG' } });
    expect(second.status).toBe(409);
  });

  it('hides other people\'s devices behind a 404', async () => {
    const stable = await loginAs(STABLE_PATIENT_EMAIL);
    const caution = await loginAs(CAUTION_PATIENT_EMAIL);
    expect((await call('GET', '/devices/CS-94281-EG/status', { token: stable.accessToken })).status).toBe(200);
    expect((await call('GET', '/devices/CS-94281-EG/status', { token: caution.accessToken })).status).toBe(404);
    expect((await call('GET', '/devices/DOES-NOT-EXIST/status', { token: caution.accessToken })).status).toBe(404);

    const doctor = await loginAs(DOCTOR_EMAIL);
    expect((await call('GET', '/devices/CS-94281-EG/status', { token: doctor.accessToken })).status).toBe(200);
  });
});

describe('demo backend: routing', () => {
  beforeEach(freshDemo);

  it('answers 404 for unknown paths and 405 for wrong methods', async () => {
    const { accessToken } = await loginAs(DOCTOR_EMAIL);
    expect((await call('GET', '/nope', { token: accessToken })).status).toBe(404);
    expect((await call('DELETE', '/doctors/me', { token: accessToken })).status).toBe(405);
  });

  it('issues tokens whose claims the router trusts only when they were really issued', async () => {
    const forged = createAccessToken({ sub: 'does-not-exist', role: 'Doctor', doctorId: 'x' }, FIXED_NOW);
    expect((await call('GET', '/doctors/me', { token: forged })).status).toBe(401);
  });
});
