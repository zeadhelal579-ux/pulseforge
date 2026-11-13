/**
 * ECG waveform math.
 *
 * Two very different situations produce the samples this module works with:
 *
 *  1. LIVE data from the backend (real device readings, shape unknown ahead
 *     of time) — `detectBeatLandmarks` finds P/Q/R/S/T positions generically
 *     via peak-detection + typical clinical proportions. This is a VISUAL
 *     approximation for labeling purposes only. The authoritative interval
 *     values (in ms) come from the backend's EcgFeatureSet
 *     (PrIntervalMs/QrsDurationMs/QtQtcMs) and are shown as separate text
 *     readouts — the frontend never presents this module's output as a
 *     clinical measurement.
 *
 *  2. The IDLE placeholder wave, shown before any live sample has arrived —
 *     `generateIdleWindow` synthesizes a stylized PQRST cycle from a closed-
 *     form function, so `computeIdleLandmarkTimes` can place labels exactly
 *     (no detection needed, since the shape is known by construction).
 */

const IDLE_HEART_RATE_BPM = 72;
const IDLE_CYCLE_SECONDS = 60 / IDLE_HEART_RATE_BPM;

// Landmark centers within one idle cycle, in seconds from cycle start —
// chosen to look like a normal sinus rhythm, not derived from real data.
const IDLE_LANDMARKS = {
  p: 0.1,
  q: 0.16,
  r: 0.18,
  s: 0.2,
  t: 0.34,
} as const;

/** One Gaussian "bump" — the building block for the synthetic P/Q/R/S/T shape. */
function gaussianBump(phase: number, center: number, width: number, amplitude: number): number {
  const distance = phase - center;
  return amplitude * Math.exp(-(distance * distance) / (2 * width * width));
}

/** Amplitude of the idle placeholder wave at a given absolute time (seconds). */
export function idleEcgValueAt(tSeconds: number): number {
  const phase = ((tSeconds % IDLE_CYCLE_SECONDS) + IDLE_CYCLE_SECONDS) % IDLE_CYCLE_SECONDS;

  const p = gaussianBump(phase, IDLE_LANDMARKS.p, 0.025, 0.15);
  const q = -gaussianBump(phase, IDLE_LANDMARKS.q, 0.008, 0.15);
  const r = gaussianBump(phase, IDLE_LANDMARKS.r, 0.01, 1.0);
  const s = -gaussianBump(phase, IDLE_LANDMARKS.s, 0.01, 0.25);
  const t = gaussianBump(phase, IDLE_LANDMARKS.t, 0.045, 0.3);

  return p + q + r + s + t;
}

/** Fills `length` samples ending at `nowSeconds`, spaced 1/sampleRateHz apart. */
export function generateIdleWindow(
  nowSeconds: number,
  length: number,
  sampleRateHz: number,
): Float32Array {
  const window = new Float32Array(length);
  for (let i = 0; i < length; i += 1) {
    const secondsAgo = (length - 1 - i) / sampleRateHz;
    window[i] = idleEcgValueAt(nowSeconds - secondsAgo);
  }
  return window;
}

/** Absolute times (seconds) of the most recent idle cycle's P/Q/R/S/T — exact,
 *  since the idle wave's shape is known by construction (see idleEcgValueAt). */
export function computeIdleLandmarkTimes(nowSeconds: number): Record<'p' | 'q' | 'r' | 's' | 't', number> {
  const cyclesSinceEpoch = Math.floor((nowSeconds - IDLE_LANDMARKS.r) / IDLE_CYCLE_SECONDS);
  const latestR = cyclesSinceEpoch * IDLE_CYCLE_SECONDS + IDLE_LANDMARKS.r;

  return {
    p: latestR - (IDLE_LANDMARKS.r - IDLE_LANDMARKS.p),
    q: latestR - (IDLE_LANDMARKS.r - IDLE_LANDMARKS.q),
    r: latestR,
    s: latestR + (IDLE_LANDMARKS.s - IDLE_LANDMARKS.r),
    t: latestR + (IDLE_LANDMARKS.t - IDLE_LANDMARKS.r),
  };
}

/** Copies a ring buffer out in chronological order (oldest → newest), starting
 *  from `writeIndex` (the slot the *next* sample would land in). */
export function unwrapRingBuffer(buffer: Float32Array, writeIndex: number): Float32Array {
  const length = buffer.length;
  const result = new Float32Array(length);
  for (let i = 0; i < length; i += 1) {
    result[i] = buffer[(writeIndex + i) % length] ?? 0;
  }
  return result;
}

export interface BeatLandmarkIndices {
  p: number;
  q: number;
  r: number;
  s: number;
  t: number;
}

/**
 * Generic landmark estimation for arbitrary (live) samples: finds the most
 * recent prominent peak (treated as R), estimates the beat's cycle length by
 * finding the previous prominent peak, then places P/Q/S/T at typical
 * clinical proportions relative to R. Returns null if the window is too flat
 * to confidently find a peak (e.g. a flatlined or all-zero buffer).
 */
export function detectBeatLandmarks(
  samples: Float32Array,
  sampleRateHz: number,
): BeatLandmarkIndices | null {
  const length = samples.length;
  if (length < sampleRateHz * 0.3) return null;

  let maxValue = -Infinity;
  for (let i = 0; i < length; i += 1) {
    const value = samples[i] ?? 0;
    if (value > maxValue) maxValue = value;
  }
  if (!Number.isFinite(maxValue) || maxValue <= 0) return null;

  const threshold = maxValue * 0.6;

  // Scan backward from the end for the last local maximum above threshold —
  // that's the most recently completed R peak.
  let rIndex = -1;
  for (let i = length - 2; i >= 1; i -= 1) {
    const value = samples[i] ?? 0;
    const prev = samples[i - 1] ?? 0;
    const next = samples[i + 1] ?? 0;
    if (value >= threshold && value >= prev && value >= next) {
      rIndex = i;
      break;
    }
  }
  if (rIndex === -1) return null;

  // Look further back for the previous peak to estimate this patient's
  // actual cycle length; fall back to a 72bpm assumption if the window
  // doesn't contain a second beat yet.
  let previousRIndex = -1;
  const minGapSamples = Math.round(sampleRateHz * 0.3); // refractory guard
  for (let i = rIndex - minGapSamples; i >= 1; i -= 1) {
    const value = samples[i] ?? 0;
    const prev = samples[i - 1] ?? 0;
    const next = samples[i + 1] ?? 0;
    if (value >= threshold && value >= prev && value >= next) {
      previousRIndex = i;
      break;
    }
  }
  const cycleSamples =
    previousRIndex !== -1 ? rIndex - previousRIndex : Math.round(sampleRateHz * IDLE_CYCLE_SECONDS);

  // Same proportions as the idle generator's landmark centers, expressed as
  // fractions of one cycle instead of absolute seconds.
  const offsetSamples = (deltaSeconds: number) =>
    Math.round((deltaSeconds / IDLE_CYCLE_SECONDS) * cycleSamples);

  return {
    p: rIndex - offsetSamples(IDLE_LANDMARKS.r - IDLE_LANDMARKS.p),
    q: rIndex - offsetSamples(IDLE_LANDMARKS.r - IDLE_LANDMARKS.q),
    r: rIndex,
    s: rIndex + offsetSamples(IDLE_LANDMARKS.s - IDLE_LANDMARKS.r),
    t: rIndex + offsetSamples(IDLE_LANDMARKS.t - IDLE_LANDMARKS.r),
  };
}
