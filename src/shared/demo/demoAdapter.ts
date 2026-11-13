/**
 * File: src/shared/demo/demoAdapter.ts
 * Layer: shared/demo (in-browser demo backend)
 * Purpose: An Axios adapter that answers requests from the in-browser demo backend instead of the network.
 * Logic:
 *  - Plugging in at the ADAPTER level (not by replacing the API modules) keeps every feature's api/*.ts file,
 *    the Authorization-header interceptor and the 401 -> refresh -> retry interceptor running exactly as they
 *    would against the real server.
 *  - A small random delay is added so loading spinners and skeletons behave like a real network.
 *  - Non-2xx results are thrown as AxiosError (with `response` and `config`), which is what the interceptors
 *    and TanStack Query expect.
 * Implementation: translate InternalAxiosRequestConfig -> DemoRequest, await handleDemoRequest, translate back.
 */
import {
  AxiosError,
  type AxiosAdapter,
  type AxiosResponse,
  type InternalAxiosRequestConfig,
} from 'axios';
import { DEMO_REFRESH_LATENCY_MS, DEMO_REQUEST_LATENCY_MS } from '@/shared/demo/demoConstants';
import { handleDemoRequest } from '@/shared/demo/demoServer';
import type { DemoMethod, DemoRequest } from '@/shared/demo/demoTypes';
import { API_BASE_URL } from '@/shared/lib/env';

const SUPPORTED_METHODS: readonly DemoMethod[] = ['GET', 'POST', 'PUT', 'PATCH', 'DELETE'];
const ABSOLUTE_URL = /^[a-z][a-z\d+\-.]*:\/\//i;
const PLACEHOLDER_ORIGIN = 'http://demo.local';

const STATUS_TEXT: Record<number, string> = {
  200: 'OK',
  201: 'Created',
  204: 'No Content',
  400: 'Bad Request',
  401: 'Unauthorized',
  403: 'Forbidden',
  404: 'Not Found',
  405: 'Method Not Allowed',
  409: 'Conflict',
  500: 'Internal Server Error',
};

/** Axios lower-cases the method; map it onto the router's upper-case union. */
function toMethod(raw: string | undefined): DemoMethod {
  const upper = (raw ?? 'get').toUpperCase();
  return SUPPORTED_METHODS.find((method) => method === upper) ?? 'GET';
}

/**
 * Reduces a request URL to the path the router knows (`/auth/login`). Handles both calls made through
 * `apiClient` (relative URL + baseURL) and the bare `axios.post` used by the silent token refresh
 * (a full URL that already contains the base).
 */
function resolvePath(url: string, baseURL: string | undefined): string {
  const joined =
    ABSOLUTE_URL.test(url) || !baseURL
      ? url
      : `${baseURL.replace(/\/+$/, '')}/${url.replace(/^\/+/, '')}`;
  const pathname = new URL(joined, PLACEHOLDER_ORIGIN).pathname;
  const basePath = new URL(API_BASE_URL, PLACEHOLDER_ORIGIN).pathname.replace(/\/+$/, '');
  if (basePath && pathname.startsWith(basePath)) return pathname.slice(basePath.length) || '/';
  return pathname;
}

/** Axios `params` object -> flat string map (null / undefined / empty values are dropped). */
function toQuery(params: unknown): Record<string, string> {
  const query: Record<string, string> = {};
  if (typeof params !== 'object' || params === null) return query;
  for (const [key, value] of Object.entries(params)) {
    if (value !== undefined && value !== null && value !== '') query[key] = String(value);
  }
  return query;
}

/** Axios serialises object bodies to a JSON string before the adapter runs; parse it back. */
function toBody(data: unknown): unknown {
  if (typeof data !== 'string') return data;
  try {
    return JSON.parse(data);
  } catch {
    return data;
  }
}

/** Builds the router request from an Axios config (without the timestamp, which is taken after the delay). */
function toDemoRequest(config: InternalAxiosRequestConfig): Omit<DemoRequest, 'nowMs'> {
  const authorization = config.headers.get('Authorization');
  return {
    method: toMethod(config.method),
    path: resolvePath(config.url ?? '', config.baseURL),
    query: toQuery(config.params),
    body: toBody(config.data),
    authorization: typeof authorization === 'string' ? authorization : null,
  };
}

/** Random delay in the configured range (shorter for the silent refresh that runs on every page load). */
function latencyFor(path: string): number {
  if (path === '/auth/refresh') return DEMO_REFRESH_LATENCY_MS;
  const { min, max } = DEMO_REQUEST_LATENCY_MS;
  return min + Math.random() * (max - min);
}

/** Promise-based timeout. */
function sleep(ms: number): Promise<void> {
  return new Promise((resolve) => setTimeout(resolve, ms));
}

/** The adapter itself: install it with `axios.create({ adapter: demoAdapter })`. */
export const demoAdapter: AxiosAdapter = async (config: InternalAxiosRequestConfig) => {
  const request = toDemoRequest(config);
  await sleep(latencyFor(request.path));

  const result = await handleDemoRequest({ ...request, nowMs: Date.now() });
  const response: AxiosResponse = {
    data: result.data,
    status: result.status,
    statusText: STATUS_TEXT[result.status] ?? '',
    headers: {},
    config,
    request: {},
  };

  const accepted = config.validateStatus
    ? config.validateStatus(response.status)
    : response.status >= 200 && response.status < 300;
  if (accepted) return response;

  throw new AxiosError(
    `Request failed with status code ${response.status}`,
    response.status >= 500 ? AxiosError.ERR_BAD_RESPONSE : AxiosError.ERR_BAD_REQUEST,
    config,
    null,
    response,
  );
};
