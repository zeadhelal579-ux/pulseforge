/**
 * File: src/shared/demo/demoAuth.ts
 * Layer: shared/demo (in-browser demo backend)
 * Purpose: Fake-but-realistic authentication primitives: unsigned JWT-shaped access tokens, salted password
 *          hashes for accounts created in the browser, and small id/salt generators.
 * Logic:
 *  - Access tokens have the same three-segment shape and the same claims the real backend issues
 *    (`sub`, `role`, `patientId` / `doctorId`, `iat`, `exp`), so the frontend's decodeJwtPayload() works
 *    unchanged. The signature segment is the literal "demo": there is no secret and no real security here.
 *  - Seeded accounts use the public demo password; accounts created in the browser store a SHA-256 hash
 *    (with a random salt) instead of the plain password, so a reused real password is never left readable
 *    in localStorage.
 * Implementation: base64url JSON for the token, Web Crypto for hashing (with a weak non-crypto fallback for
 *    insecure contexts where crypto.subtle is unavailable).
 */
import { ACCESS_TOKEN_TTL_MS, DEMO_PASSWORD } from '@/shared/demo/demoConstants';
import { seedFromString } from '@/shared/demo/demoMath';
import type { DemoCredential } from '@/shared/demo/demoTypes';
import type { UserRole } from '@/shared/types/api';

export interface DemoTokenClaims {
  sub: string;
  role: UserRole;
  patientId?: string;
  doctorId?: string;
  /** Issued-at / expiry in epoch SECONDS, as in a real JWT. */
  iat: number;
  exp: number;
}

/** Returns the Web Crypto object when the runtime provides one. */
function getCrypto(): Crypto | undefined {
  return typeof globalThis.crypto === 'undefined' ? undefined : globalThis.crypto;
}

/** Encodes text as base64url (the JWT alphabet, no padding). Input is ASCII-only by construction. */
function toBase64Url(text: string): string {
  return btoa(text).replace(/\+/g, '-').replace(/\//g, '_').replace(/=+$/, '');
}

/** Decodes a base64url segment back to text. */
function fromBase64Url(segment: string): string {
  return atob(segment.replace(/-/g, '+').replace(/_/g, '/'));
}

/** Creates a JWT-shaped access token valid for ACCESS_TOKEN_TTL_MS. */
export function createAccessToken(
  claims: Omit<DemoTokenClaims, 'iat' | 'exp'>,
  nowMs: number,
): string {
  const iat = Math.floor(nowMs / 1000);
  const payload: DemoTokenClaims = {
    ...claims,
    iat,
    exp: iat + Math.floor(ACCESS_TOKEN_TTL_MS / 1000),
  };
  const header = toBase64Url(JSON.stringify({ alg: 'none', typ: 'JWT' }));
  return `${header}.${toBase64Url(JSON.stringify(payload))}.demo`;
}

/** Parses a token created by createAccessToken: claims when valid, 'expired' when past `exp`, else null. */
export function parseAccessToken(token: string, nowMs: number): DemoTokenClaims | 'expired' | null {
  const segments = token.split('.');
  if (segments.length !== 3 || segments[2] !== 'demo') return null;

  try {
    const claims = JSON.parse(fromBase64Url(segments[1] ?? '')) as Partial<DemoTokenClaims>;
    const hasValidShape =
      typeof claims.sub === 'string' &&
      (claims.role === 'Patient' || claims.role === 'Doctor') &&
      typeof claims.iat === 'number' &&
      typeof claims.exp === 'number';
    if (!hasValidShape) return null;
    if ((claims.exp ?? 0) * 1000 <= nowMs) return 'expired';

    return {
      sub: claims.sub as string,
      role: claims.role as UserRole,
      patientId: typeof claims.patientId === 'string' ? claims.patientId : undefined,
      doctorId: typeof claims.doctorId === 'string' ? claims.doctorId : undefined,
      iat: claims.iat as number,
      exp: claims.exp as number,
    };
  } catch {
    return null;
  }
}

/** Extracts the token from an `Authorization: Bearer <token>` header value. */
export function bearerToken(authorization: string | null): string | null {
  if (!authorization) return null;
  const match = /^Bearer\s+(\S+)$/i.exec(authorization.trim());
  return match?.[1] ?? null;
}

/** Random id (UUID when available) for records created at runtime. */
export function newId(): string {
  const cryptoApi = getCrypto();
  if (cryptoApi && typeof cryptoApi.randomUUID === 'function') return cryptoApi.randomUUID();
  return `id-${Date.now().toString(36)}-${Math.floor(Math.random() * 1e9).toString(36)}`;
}

/** Random salt (16 hex characters) for password hashing. */
export function createSalt(): string {
  const bytes = new Uint8Array(8);
  const cryptoApi = getCrypto();
  if (cryptoApi) {
    cryptoApi.getRandomValues(bytes);
  } else {
    for (let i = 0; i < bytes.length; i += 1) bytes[i] = Math.floor(Math.random() * 256);
  }
  return Array.from(bytes, (byte) => byte.toString(16).padStart(2, '0')).join('');
}

/** Non-cryptographic stand-in used only when crypto.subtle is unavailable (insecure http contexts). */
function fallbackHash(text: string): string {
  let hex = '';
  for (let round = 0; round < 4; round += 1) {
    hex += seedFromString(`${round}:${text}`).toString(16).padStart(8, '0');
  }
  return hex;
}

/** SHA-256 hex digest of `salt:password`. */
export async function hashPassword(password: string, salt: string): Promise<string> {
  const subtle = getCrypto()?.subtle;
  const input = `${salt}:${password}`;
  if (!subtle) return fallbackHash(input);

  const digest = await subtle.digest('SHA-256', new TextEncoder().encode(input));
  return Array.from(new Uint8Array(digest), (byte) => byte.toString(16).padStart(2, '0')).join('');
}

/** Builds the stored credential for an account created in the browser. */
export async function createHashedCredential(password: string): Promise<DemoCredential> {
  const salt = createSalt();
  return { kind: 'hashed', salt, hash: await hashPassword(password, salt) };
}

/** Checks a password against a stored credential. */
export async function verifyCredential(
  credential: DemoCredential,
  password: string,
): Promise<boolean> {
  if (credential.kind === 'demo') return password === DEMO_PASSWORD;
  return (await hashPassword(password, credential.salt)) === credential.hash;
}
