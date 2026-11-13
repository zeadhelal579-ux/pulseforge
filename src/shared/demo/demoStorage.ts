/**
 * File: src/shared/demo/demoStorage.ts
 * Layer: shared/demo (in-browser demo backend)
 * Purpose: Persists the demo database and the emulated refresh session in localStorage ("data is stored in
 *          this browser only"), and offers the "Reset demo data" operation.
 * Logic:
 *  - The database is read fresh on every request, so two tabs stay consistent and nothing goes stale.
 *  - A missing, corrupted or old-schema database is replaced by a fresh seed instead of crashing the demo.
 *  - If localStorage is blocked (private mode) or full, the demo transparently keeps working from memory.
 * Implementation: thin guarded wrappers around localStorage plus a Map fallback.
 */
import { DEMO_DB_VERSION, DEMO_SESSION_KEY, DEMO_STORAGE_KEY } from '@/shared/demo/demoConstants';
import { createSeedDatabase } from '@/shared/demo/demoSeed';
import type { DemoDatabase, DemoSession } from '@/shared/demo/demoTypes';

const memoryStore = new Map<string, string>();
let storageBroken = false;

/** Reads a raw string from localStorage, or from memory when storage is unavailable. */
function readItem(key: string): string | null {
  if (!storageBroken && typeof localStorage !== 'undefined') {
    try {
      return localStorage.getItem(key);
    } catch {
      storageBroken = true; // blocked storage: switch to the in-memory fallback
    }
  }
  return memoryStore.get(key) ?? null;
}

/** Writes a raw string to localStorage, falling back to memory if the write is rejected. */
function writeItem(key: string, value: string): void {
  if (!storageBroken && typeof localStorage !== 'undefined') {
    try {
      localStorage.setItem(key, value);
      return;
    } catch {
      storageBroken = true; // quota exceeded or blocked: keep working from memory
    }
  }
  memoryStore.set(key, value);
}

/** Removes a key from both localStorage and the memory fallback. */
function removeItem(key: string): void {
  memoryStore.delete(key);
  if (typeof localStorage === 'undefined') return;
  try {
    localStorage.removeItem(key);
  } catch {
    // Nothing to clean up if storage is blocked.
  }
}

/** Cheap structural check so a corrupted or outdated blob is never trusted. */
function isDatabase(value: unknown): value is DemoDatabase {
  if (typeof value !== 'object' || value === null) return false;
  const record = value as Record<string, unknown>;
  const collections = ['users', 'patients', 'doctors', 'assignments', 'devices', 'manualAlerts'];
  return (
    record.version === DEMO_DB_VERSION && collections.every((key) => Array.isArray(record[key]))
  );
}

/** Loads the database, seeding it on first use (or after corruption / a schema change). */
export function loadDemoDb(nowMs: number = Date.now()): DemoDatabase {
  const raw = readItem(DEMO_STORAGE_KEY);
  if (raw) {
    try {
      const parsed: unknown = JSON.parse(raw);
      if (isDatabase(parsed)) return parsed;
    } catch {
      // Corrupted JSON: fall through and reseed.
    }
  }
  const seeded = createSeedDatabase(nowMs);
  saveDemoDb(seeded);
  return seeded;
}

/** Persists the whole database. */
export function saveDemoDb(db: DemoDatabase): void {
  writeItem(DEMO_STORAGE_KEY, JSON.stringify(db));
}

/** Deletes the database and the session; the next request reseeds from scratch. */
export function resetDemoData(): void {
  removeItem(DEMO_STORAGE_KEY);
  removeItem(DEMO_SESSION_KEY);
}

/** Reads the emulated refresh session, returning null when absent, malformed or expired. */
export function readDemoSession(nowMs: number): DemoSession | null {
  const raw = readItem(DEMO_SESSION_KEY);
  if (!raw) return null;
  try {
    const parsed = JSON.parse(raw) as Partial<DemoSession>;
    if (typeof parsed.userId !== 'string' || typeof parsed.expiresAt !== 'number') return null;
    if (parsed.expiresAt <= nowMs) return null;
    return { userId: parsed.userId, expiresAt: parsed.expiresAt };
  } catch {
    return null;
  }
}

/** Stores the emulated refresh session. */
export function writeDemoSession(session: DemoSession): void {
  writeItem(DEMO_SESSION_KEY, JSON.stringify(session));
}

/** Ends the emulated session (logout). */
export function clearDemoSession(): void {
  removeItem(DEMO_SESSION_KEY);
}
