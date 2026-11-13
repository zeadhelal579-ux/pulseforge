import { describe, expect, it } from 'vitest';
import {
  computeIdleLandmarkTimes,
  detectBeatLandmarks,
  generateIdleWindow,
  idleEcgValueAt,
  unwrapRingBuffer,
} from '@/features/patient-dashboard/components/ecgWaveformMath';

describe('unwrapRingBuffer', () => {
  it('reorders a wrapped ring buffer into chronological order', () => {
    // Buffer holds [3,4,5,1,2] with writeIndex=3 (next write lands on index 3,
    // meaning the oldest sample is at index 3) -> chronological: [1,2,3,4,5]
    const buffer = new Float32Array([3, 4, 5, 1, 2]);
    const result = unwrapRingBuffer(buffer, 3);
    expect(Array.from(result)).toEqual([1, 2, 3, 4, 5]);
  });

  it('returns the buffer unchanged when writeIndex is 0', () => {
    const buffer = new Float32Array([1, 2, 3]);
    expect(Array.from(unwrapRingBuffer(buffer, 0))).toEqual([1, 2, 3]);
  });
});

describe('idleEcgValueAt', () => {
  it('is periodic — the same phase within a cycle repeats', () => {
    const a = idleEcgValueAt(0.18); // R peak of the first cycle
    const cycleSeconds = 60 / 72;
    const b = idleEcgValueAt(0.18 + cycleSeconds); // same phase, next cycle
    expect(a).toBeCloseTo(b, 5);
  });

  it('the R peak (phase 0.18) is the highest point in a cycle', () => {
    const rValue = idleEcgValueAt(0.18);
    const sampledOtherPhases = [0, 0.05, 0.1, 0.25, 0.4, 0.6].map(idleEcgValueAt);
    for (const other of sampledOtherPhases) {
      expect(rValue).toBeGreaterThan(other);
    }
  });
});

describe('generateIdleWindow', () => {
  it('produces exactly `length` samples', () => {
    const window = generateIdleWindow(10, 1000, 250);
    expect(window.length).toBe(1000);
  });

  it('the last sample corresponds to `nowSeconds`', () => {
    const now = 12.345;
    const window = generateIdleWindow(now, 500, 250);
    expect(window[window.length - 1]).toBeCloseTo(idleEcgValueAt(now), 5);
  });
});

describe('computeIdleLandmarkTimes', () => {
  it('returns landmarks in chronological order (P < Q < R < S < T)', () => {
    const landmarks = computeIdleLandmarkTimes(100);
    expect(landmarks.p).toBeLessThan(landmarks.q);
    expect(landmarks.q).toBeLessThan(landmarks.r);
    expect(landmarks.r).toBeLessThan(landmarks.s);
    expect(landmarks.s).toBeLessThan(landmarks.t);
  });

  it('the R landmark time is never in the future relative to `now`', () => {
    const now = 42.7;
    const landmarks = computeIdleLandmarkTimes(now);
    expect(landmarks.r).toBeLessThanOrEqual(now);
  });
});

describe('detectBeatLandmarks', () => {
  it('returns null for a silent (all-zero) buffer', () => {
    const samples = new Float32Array(300);
    expect(detectBeatLandmarks(samples, 250)).toBeNull();
  });

  it('returns null when the window is too short for the sample rate', () => {
    const samples = new Float32Array(10);
    expect(detectBeatLandmarks(samples, 250)).toBeNull();
  });

  it('finds a peak matching an injected R spike', () => {
    const sampleRate = 250;
    const samples = new Float32Array(sampleRate * 2); // 2 seconds
    const spikeIndex = 300;
    samples[spikeIndex] = 1.0; // single sharp spike
    const landmarks = detectBeatLandmarks(samples, sampleRate);
    expect(landmarks).not.toBeNull();
    expect(landmarks?.r).toBe(spikeIndex);
  });

  it('orders P < Q < R < S < T for a detected beat', () => {
    const sampleRate = 250;
    const samples = generateIdleWindow(4, sampleRate * 4, sampleRate); // several real-shaped cycles
    const landmarks = detectBeatLandmarks(samples, sampleRate);
    expect(landmarks).not.toBeNull();
    if (landmarks) {
      expect(landmarks.p).toBeLessThan(landmarks.q);
      expect(landmarks.q).toBeLessThan(landmarks.r);
      expect(landmarks.r).toBeLessThan(landmarks.s);
      expect(landmarks.s).toBeLessThan(landmarks.t);
    }
  });
});
