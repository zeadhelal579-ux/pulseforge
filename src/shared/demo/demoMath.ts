/**
 * File: src/shared/demo/demoMath.ts
 * Layer: shared/demo (in-browser demo backend)
 * Purpose: Small deterministic math helpers (hashing, smooth noise, seeded RNG) used by the simulators.
 * Logic: The demo must return the SAME heart rate for the same patient and the same moment whether the
 *        value is read through REST or pushed over the hub, so every "random" value is a pure function of
 *        (seed, time bucket) instead of Math.random().
 * Implementation: integer hashing (FNV-1a, murmur-style finalizer), cosine-eased value noise, mulberry32.
 */

/** Limits a number to the inclusive range [min, max]. */
export function clamp(value: number, min: number, max: number): number {
  return Math.min(max, Math.max(min, value));
}

/** Rounds to a fixed number of decimal places. */
export function roundTo(value: number, digits: number): number {
  const factor = 10 ** digits;
  return Math.round(value * factor) / factor;
}

/** FNV-1a 32-bit hash of a string; used to derive a stable numeric seed from an id. */
export function seedFromString(text: string): number {
  let hash = 0x811c9dc5;
  for (let i = 0; i < text.length; i += 1) {
    hash ^= text.charCodeAt(i);
    hash = Math.imul(hash, 0x01000193);
  }
  return hash >>> 0;
}

/** Deterministic pseudo-random number in [0, 1) for a (seed, index) pair. */
export function hash01(seed: number, index: number): number {
  let h = (seed ^ Math.imul(index | 0, 0x9e3779b1)) >>> 0;
  h = Math.imul(h ^ (h >>> 16), 0x85ebca6b) >>> 0;
  h = Math.imul(h ^ (h >>> 13), 0xc2b2ae35) >>> 0;
  return ((h ^ (h >>> 16)) >>> 0) / 4294967296;
}

/**
 * Smooth noise in [0, 1): hashes the two neighbouring time buckets of width `periodMs` and eases between
 * them, so a series sampled at any resolution looks continuous instead of jumping bucket to bucket.
 */
export function valueNoise(seed: number, tMs: number, periodMs: number): number {
  const position = tMs / periodMs;
  const index = Math.floor(position);
  const fraction = position - index;
  const eased = fraction * fraction * (3 - 2 * fraction);
  const a = hash01(seed, index);
  const b = hash01(seed, index + 1);
  return a + (b - a) * eased;
}

/** Seeded sequential random generator (mulberry32); used where a stream of values is needed. */
export function createRng(seed: number): () => number {
  let state = seed >>> 0;
  return () => {
    state = (state + 0x6d2b79f5) >>> 0;
    let t = state;
    t = Math.imul(t ^ (t >>> 15), t | 1);
    t ^= t + Math.imul(t ^ (t >>> 7), t | 61);
    return ((t ^ (t >>> 14)) >>> 0) / 4294967296;
  };
}
