import { beforeEach, describe, expect, it } from 'vitest';
import { DEMO_DB_VERSION, DEMO_STORAGE_KEY } from '@/shared/demo/demoConstants';
import {
  clearDemoSession,
  loadDemoDb,
  readDemoSession,
  resetDemoData,
  saveDemoDb,
  writeDemoSession,
} from '@/shared/demo/demoStorage';
import { FIXED_NOW, freshDemo } from '@/test/demoTestUtils';

describe('demo storage', () => {
  beforeEach(freshDemo);

  it('seeds on first use and persists changes', () => {
    const db = loadDemoDb(FIXED_NOW);
    expect(db.version).toBe(DEMO_DB_VERSION);
    expect(db.doctors).toHaveLength(1);
    expect(db.patients.length).toBeGreaterThanOrEqual(12);

    const first = db.patients[0];
    if (!first) throw new Error('seed has no patients');
    first.fullName = 'Renamed';
    saveDemoDb(db);
    expect(loadDemoDb(FIXED_NOW).patients[0]?.fullName).toBe('Renamed');
  });

  it('recovers from corrupted or outdated data by reseeding', () => {
    localStorage.setItem(DEMO_STORAGE_KEY, '{not json');
    expect(loadDemoDb(FIXED_NOW).doctors).toHaveLength(1);

    localStorage.setItem(DEMO_STORAGE_KEY, JSON.stringify({ version: 0, users: [] }));
    expect(loadDemoDb(FIXED_NOW).version).toBe(DEMO_DB_VERSION);
  });

  it('resets both the database and the session', () => {
    const db = loadDemoDb(FIXED_NOW);
    const first = db.patients[0];
    if (!first) throw new Error('seed has no patients');
    first.fullName = 'Renamed';
    saveDemoDb(db);
    writeDemoSession({ userId: 'u', expiresAt: FIXED_NOW + 1000 });

    resetDemoData();
    expect(readDemoSession(FIXED_NOW)).toBeNull();
    expect(loadDemoDb(FIXED_NOW).patients[0]?.fullName).not.toBe('Renamed');
  });

  it('treats an expired session as signed out', () => {
    writeDemoSession({ userId: 'u', expiresAt: FIXED_NOW + 1000 });
    expect(readDemoSession(FIXED_NOW)?.userId).toBe('u');
    expect(readDemoSession(FIXED_NOW + 2000)).toBeNull();
    clearDemoSession();
    expect(readDemoSession(FIXED_NOW)).toBeNull();
  });
});
