import { describe, expect, it } from 'vitest';
import { detectBeatLandmarks } from '@/features/patient-dashboard/components/ecgWaveformMath';
import { EcgGenerator } from '@/shared/demo/ecgSynth';

const SAMPLE_RATE_HZ = 250;
const WINDOW = SAMPLE_RATE_HZ * 4; // the live waveform keeps a 4-second window

/** Highest value within +-radius samples of an index (bounds-safe). */
function peakNear(samples: Float32Array, index: number, radius: number): number {
  let best = -Infinity;
  for (let i = Math.max(0, index - radius); i <= Math.min(samples.length - 1, index + radius); i += 1) {
    best = Math.max(best, samples[i] ?? -Infinity);
  }
  return best;
}

describe('simulated ECG vs the waveform landmark detector', () => {
  it.each([55, 72, 100, 118])('places P, R and T labels on real features at %i bpm', (heartRate) => {
    const generator = new EcgGenerator(
      { heartRate, stElevation: 0, qrsWidthScale: 1, noise: 0.015, irregularity: 0.02, ectopicChance: 0 },
      7,
    );
    generator.nextBatch(SAMPLE_RATE_HZ * 3); // warm up
    const window = Float32Array.from(generator.nextBatch(WINDOW));

    const landmarks = detectBeatLandmarks(window, SAMPLE_RATE_HZ);
    expect(landmarks).not.toBeNull();
    if (!landmarks) return;

    expect(window[landmarks.r] ?? 0).toBeGreaterThan(0.8); // R label sits on the R peak
    expect(peakNear(window, landmarks.p, 8)).toBeGreaterThan(0.08); // a P wave exists where "P" is drawn
    if (landmarks.t < WINDOW - 8) {
      expect(peakNear(window, landmarks.t, 10)).toBeGreaterThan(0.18); // a T wave exists where "T" is drawn
    }
  });
});
