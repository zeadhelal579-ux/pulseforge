/**
 * Decodes (does NOT verify) a JWT's payload segment. Signature verification
 * happens on the backend on every request — this is purely so the frontend
 * can read the `sub`/`role` claims already described in
 * backend-database-architecture.md §8 ("issues a JWT access token... contains
 * sub, role") right after a silent refresh, without a dedicated /auth/me
 * endpoint.
 */
export interface DecodedAccessToken {
  sub: string;
  role: 'Patient' | 'Doctor';
  [claim: string]: unknown;
}

export function decodeJwtPayload(token: string): DecodedAccessToken | null {
  try {
    const payloadSegment = token.split('.')[1];
    if (!payloadSegment) return null;

    // JWTs use base64url, not standard base64 — swap the two characters that differ.
    const base64 = payloadSegment.replace(/-/g, '+').replace(/_/g, '/');
    const json = decodeURIComponent(
      atob(base64)
        .split('')
        .map((char) => '%' + char.charCodeAt(0).toString(16).padStart(2, '0'))
        .join(''),
    );
    return JSON.parse(json) as DecodedAccessToken;
  } catch {
    return null;
  }
}
