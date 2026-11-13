import { describe, expect, it } from 'vitest';
import {
  bearerToken,
  createAccessToken,
  createHashedCredential,
  hashPassword,
  parseAccessToken,
  verifyCredential,
} from '@/shared/demo/demoAuth';
import { decodeJwtPayload } from '@/shared/utils/jwt';
import { FIXED_NOW } from '@/test/demoTestUtils';

describe('demo access tokens', () => {
  it('round-trips claims and decodes with the frontend\'s own JWT reader', () => {
    const token = createAccessToken({ sub: 'user-1', role: 'Patient', patientId: 'p-1' }, FIXED_NOW);
    expect(parseAccessToken(token, FIXED_NOW)).toMatchObject({ sub: 'user-1', role: 'Patient', patientId: 'p-1' });

    const decoded = decodeJwtPayload(token);
    expect(decoded?.sub).toBe('user-1');
    expect(decoded?.role).toBe('Patient');
  });

  it('expires after fifteen minutes', () => {
    const token = createAccessToken({ sub: 'u', role: 'Doctor', doctorId: 'd' }, FIXED_NOW);
    expect(parseAccessToken(token, FIXED_NOW + 14 * 60_000)).not.toBe('expired');
    expect(parseAccessToken(token, FIXED_NOW + 15 * 60_000)).toBe('expired');
  });

  it('rejects malformed tokens and tampered payloads', () => {
    expect(parseAccessToken('', FIXED_NOW)).toBeNull();
    expect(parseAccessToken('a.b', FIXED_NOW)).toBeNull();
    expect(parseAccessToken('a.b.c', FIXED_NOW)).toBeNull();
    const token = createAccessToken({ sub: 'u', role: 'Doctor', doctorId: 'd' }, FIXED_NOW);
    const [header, , signature] = token.split('.');
    const bogusPayload = btoa(JSON.stringify({ sub: 'u', role: 'Admin', iat: 1, exp: 9_999_999_999 }));
    expect(parseAccessToken(`${header}.${bogusPayload}.${signature}`, FIXED_NOW)).toBeNull();
  });

  it('extracts bearer tokens', () => {
    expect(bearerToken('Bearer abc.def.ghi')).toBe('abc.def.ghi');
    expect(bearerToken('bearer   x')).toBe('x');
    expect(bearerToken('Basic abc')).toBeNull();
    expect(bearerToken(null)).toBeNull();
  });
});

describe('demo credentials', () => {
  it('stores a salted hash, never the password, and verifies it', async () => {
    const credential = await createHashedCredential('Secret123');
    expect(JSON.stringify(credential)).not.toContain('Secret123');
    expect(await verifyCredential(credential, 'Secret123')).toBe(true);
    expect(await verifyCredential(credential, 'secret123')).toBe(false);
  });

  it('salts every hash differently', async () => {
    const [a, b] = await Promise.all([createHashedCredential('same'), createHashedCredential('same')]);
    expect(a).not.toEqual(b);
    expect(await hashPassword('same', 'salt-1')).not.toBe(await hashPassword('same', 'salt-2'));
  });

  it('accepts the public demo password for seeded accounts only', async () => {
    expect(await verifyCredential({ kind: 'demo' }, 'Demo@1234')).toBe(true);
    expect(await verifyCredential({ kind: 'demo' }, 'Demo@12345')).toBe(false);
  });
});
