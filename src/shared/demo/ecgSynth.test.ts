import { describe, expect, it } from 'vitest';
import { EcgGenerator, type EcgParams } from '@/shared/demo/ecgSynth';

const SAMPLE_RATE_HZ = 250;

const STABLE: EcgParams = {
  heartRate: 72,
  stElevation: 0,
  qrsWidthScale: 1,
  noise: 0.015,
  irregularity: 0.03,
  ectopicChance: 0,
};

/** Counts R peaks: local maxima above the threshold, at least 0.24 s apart. */
function countRPeaks(samples: number[], threshold = 0.6): number {
  let peaks = 0;
  let lastPeak = -Infinity;
  for (let i = 1; i < samples.length - 1; i += 1) {
    const value = samples[i] ?? 0;
    if (value > threshold && value >= (samples[i - 1] ?? 0) && value > (samples[i + 1] ?? 0)) {
      if (i - lastPeak > SAMPLE_RATE_HZ * 0.24) peaks += 1;
      lastPeak = i;
    }
  }
  return peaks;
}

function mean(samples: number[]): number {
  return samples.reduce((sum, value) => sum + value, 0) / samples.length;
}

describe('EcgGenerator', () => {
  it('produces finite, bounded samples', () => {
    const samples = new EcgGenerator(STABLE, 1).nextBatch(SAMPLE_RATE_HZ * 20);
    expect(samples.every((value) => Number.isFinite(value) && value > -1.5 && value < 2)).toBe(true);
  });

  it('beats at the requested heart rate', () => {
    const at60 = countRPeaks(new EcgGenerator({ ...STABLE, heartRate: 60 }, 2).nextBatch(SAMPLE_RATE_HZ * 30));
    const at120 = countRPeaks(new EcgGenerator({ ...STABLE, heartRate: 120 }, 2).nextBatch(SAMPLE_RATE_HZ * 30));
    expect(Math.abs(at60 - 30)).toBeLessThanOrEqual(2);
    expect(Math.abs(at120 - 60)).toBeLessThanOrEqual(3);
  });

  it('follows a heart-rate change from the next beat onward', () => {
    const generator = new EcgGenerator({ ...STABLE, heartRate: 60 }, 3);
    generator.nextBatch(SAMPLE_RATE_HZ * 5);
    generator.setParams({ ...STABLE, heartRate: 120 });
    generator.nextBatch(SAMPLE_RATE_HZ * 2); // let the current slow beat finish
    const peaks = countRPeaks(generator.nextBatch(SAMPLE_RATE_HZ * 10));
    expect(Math.abs(peaks - 20)).toBeLessThanOrEqual(2);
  });

  it('is reproducible for the same seed and differs between seeds', () => {
    const a = new EcgGenerator(STABLE, 42).nextBatch(500);
    const b = new EcgGenerator(STABLE, 42).nextBatch(500);
    const c = new EcgGenerator(STABLE, 43).nextBatch(500);
    expect(a).toEqual(b);
    expect(a).not.toEqual(c);
  });

  it('raises the ST segment when ST elevation is requested', () => {
    const flat = mean(new EcgGenerator(STABLE, 5).nextBatch(SAMPLE_RATE_HZ * 20));
    const raised = mean(new EcgGenerator({ ...STABLE, stElevation: 0.35 }, 5).nextBatch(SAMPLE_RATE_HZ * 20));
    expect(raised - flat).toBeGreaterThan(0.02);
  });

  it('inserts premature beats (and keeps the average rate) when ectopic beats are enabled', () => {
    const samples = new EcgGenerator({ ...STABLE, ectopicChance: 0.3, irregularity: 0 }, 9).nextBatch(
      SAMPLE_RATE_HZ * 60,
    );
    const peaks = countRPeaks(samples);
    // Each premature beat is followed by a compensatory pause, so the average rate stays close to 72 bpm.
    expect(Math.abs(peaks - 72)).toBeLessThanOrEqual(6);
    // A wide ectopic complex reaches above the normal 1.0 mV R wave.
    expect(Math.max(...samples)).toBeGreaterThan(1.2);
  });
});
