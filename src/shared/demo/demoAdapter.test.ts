import { AxiosError, type InternalAxiosRequestConfig } from 'axios';
import { beforeEach, describe, expect, it } from 'vitest';
import { demoAdapter } from '@/shared/demo/demoAdapter';
import { DOCTOR_EMAIL, freshDemo } from '@/test/demoTestUtils';
import type { AuthResponse, PaginatedResponse, PatientListItem } from '@/shared/types/api';

/** A header bag with the one method the adapter uses (`get`), like Axios' AxiosHeaders. */
function headers(values: Record<string, string> = {}): InternalAxiosRequestConfig['headers'] {
  return { get: (name: string) => values[name] ?? null } as unknown as InternalAxiosRequestConfig['headers'];
}

/** What Axios hands to an adapter: JSON-stringified body, params object, merged defaults. */
function config(partial: Partial<InternalAxiosRequestConfig>): InternalAxiosRequestConfig {
  return {
    headers: headers(),
    validateStatus: (status: number) => status >= 200 && status < 300,
    ...partial,
  } as InternalAxiosRequestConfig;
}

describe('demoAdapter', () => {
  beforeEach(freshDemo);

  it('answers a relative URL + baseURL request like a real server would', async () => {
    const response = await demoAdapter(
      config({
        url: '/auth/login',
        baseURL: '/api',
        method: 'post',
        data: JSON.stringify({ email: DOCTOR_EMAIL, password: 'Demo@1234' }),
      }),
    );
    expect(response.status).toBe(200);
    expect((response.data as AuthResponse).user.role).toBe('Doctor');
    expect(response.statusText).toBe('OK');
  });

  it('understands the full URL used by the bare silent-refresh call', async () => {
    await demoAdapter(
      config({
        url: '/auth/login',
        baseURL: '/api',
        method: 'post',
        data: JSON.stringify({ email: DOCTOR_EMAIL, password: 'Demo@1234' }),
      }),
    );
    for (const url of ['/api/auth/refresh', 'https://localhost:5001/api/auth/refresh']) {
      const response = await demoAdapter(config({ url, method: 'post' }));
      expect(response.status).toBe(200);
    }
  });

  it('turns non-2xx results into an AxiosError carrying the response and the config', async () => {
    const failing = config({
      url: '/auth/login',
      baseURL: '/api',
      method: 'post',
      data: JSON.stringify({ email: DOCTOR_EMAIL, password: 'wrong-password' }),
    });
    const error = await demoAdapter(failing).catch((caught: unknown) => caught);
    expect(error).toBeInstanceOf(AxiosError);
    const axiosError = error as AxiosError;
    expect(axiosError.response?.status).toBe(401);
    expect(axiosError.config).toBe(failing);
    expect(axiosError.code).toBe('ERR_BAD_REQUEST');
  });

  it('passes the bearer token and the query parameters through, dropping empty ones', async () => {
    const login = await demoAdapter(
      config({
        url: '/auth/login',
        baseURL: '/api',
        method: 'post',
        data: JSON.stringify({ email: DOCTOR_EMAIL, password: 'Demo@1234' }),
      }),
    );
    const token = (login.data as AuthResponse).accessToken;

    const response = await demoAdapter(
      config({
        url: '/doctors/me/patients',
        baseURL: '/api',
        method: 'get',
        params: { status: 'critical', search: '', page: 1 },
        headers: headers({ Authorization: `Bearer ${token}` }),
      }),
    );
    const page = response.data as PaginatedResponse<PatientListItem>;
    expect(page.totalCount).toBe(2);

    const anonymous = await demoAdapter(
      config({ url: '/doctors/me/patients', baseURL: '/api', method: 'get' }),
    ).catch((caught: unknown) => caught);
    expect((anonymous as AxiosError).response?.status).toBe(401);
  });
});
