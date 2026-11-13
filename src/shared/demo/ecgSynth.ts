/**
 * File: src/shared/demo/ecgSynth.ts
 * Layer: shared/demo (in-browser demo backend)
 * Purpose: Generates a believable, continuous single-lead ECG trace (P-QRS-T complexes) sample by sample.
 * Logic:
 *  - Each beat is a sum of Gaussian bumps (P, Q, R, S, T) placed at fixed FRACTIONS of the beat's RR interval.
 *    The fractions equal the proportions the waveform component's landmark detector assumes
 *    (ecgWaveformMath.ts), so its P/Q/R/S/T letters land on the right features at any heart rate.
 *  - Scenario knobs: ST elevation, a wider QRS, beat-to-beat irregularity, premature (ectopic) beats followed
 *    by a compensatory pause, and electrode-style noise.
 *  - Output is "mV-like" (R peak ~ 1.0); the waveform component auto-scales, so absolute units do not matter.
 * Implementation: a small stateful class advanced one sample at a time (250 Hz), seeded for reproducibility.
 */
import { DEMO_ECG_SAMPLE_RATE_HZ } from '@/shared/demo/demoConstants';
import { clamp, createRng } from '@/shared/demo/demoMath';

export interface EcgParams {
  /** Beats per minute (clamped to 30-220 internally). */
  heartRate: number;
  /** Height of the ST-segment bump in mV-like units; 0 = isoelectric, ~0.3 = clear elevation. */
  stElevation: number;
  /** 1 = normal QRS; larger values widen Q, R and S. */
  qrsWidthScale: number;
  /** Amplitude of per-sample noise (electrode / muscle artefact). */
  noise: number;
  /** Beat-to-beat RR variability as a fraction (0.03 = +-3%). */
  irregularity: number;
  /** Probability that a given beat is a premature ventricular beat. */
  ectopicChance: number;
}

// Landmark centres as a fraction of one RR interval: the idle placeholder wave's landmark times
// (P .10s, Q .16s, R .18s, S .20s, T .34s) divided by its 0.8333 s cycle.
const P_CENTER = 0.12;
const Q_CENTER = 0.192;
const R_CENTER = 0.216;
const S_CENTER = 0.24;
const T_CENTER = 0.408;
const ST_CENTER = 0.31;

/** One Gaussian bump evaluated at beat phase `u` (0-1). */
function bump(u: number, center: number, width: number, amplitude: number): number {
  const distance = u - center;
  return amplitude * Math.exp(-(distance * distance) / (2 * width * width));
}

export class EcgGenerator {
  private readonly rng: () => number;
  private readonly sampleRateHz: number;
  private params: EcgParams;
  private phaseSeconds = 0;
  private rrSeconds = 1;
  private elapsedSeconds = 0;
  private ectopicBeat = false;
  private nextIsEctopic = false;

  /** Creates a generator; `seed` makes the beat-to-beat variation reproducible. */
  constructor(params: EcgParams, seed: number, sampleRateHz: number = DEMO_ECG_SAMPLE_RATE_HZ) {
    this.params = params;
    this.sampleRateHz = sampleRateHz;
    this.rng = createRng(seed);
    this.startBeat();
  }

  /** Updates the scenario knobs; a new heart rate takes effect from the next beat. */
  setParams(params: EcgParams): void {
    this.params = params;
  }

  /** Produces the next sample and advances time by 1 / sampleRateHz seconds. */
  next(): number {
    const u = this.phaseSeconds / this.rrSeconds;
    const value = this.beatShape(u) + this.baseline() + (this.rng() - 0.5) * 2 * this.params.noise;

    this.phaseSeconds += 1 / this.sampleRateHz;
    this.elapsedSeconds += 1 / this.sampleRateHz;
    if (this.phaseSeconds >= this.rrSeconds) {
      this.phaseSeconds -= this.rrSeconds;
      this.startBeat();
    }
    return value;
  }

  /** Convenience for tests and previews: the next `count` samples. */
  nextBatch(count: number): number[] {
    const samples: number[] = [];
    for (let i = 0; i < count; i += 1) samples.push(this.next());
    return samples;
  }

  /**
   * Decides the length of the beat that starts now. A premature beat is produced by shortening the PREVIOUS
   * beat (so the next one arrives early) and giving the premature beat a longer, compensatory interval.
   */
  private startBeat(): void {
    const baseRr = 60 / clamp(this.params.heartRate, 30, 220);
    this.ectopicBeat = this.nextIsEctopic;
    this.nextIsEctopic = !this.ectopicBeat && this.rng() < this.params.ectopicChance;

    if (this.ectopicBeat) {
      this.rrSeconds = baseRr * 1.25;
    } else if (this.nextIsEctopic) {
      this.rrSeconds = baseRr * 0.75;
    } else {
      const jitter = (this.rng() - 0.5) * 2 * this.params.irregularity;
      this.rrSeconds = baseRr * (1 + jitter);
    }
  }

  /** The P-QRS-T-ST complex at beat phase `u`. */
  private beatShape(u: number): number {
    const ectopic = this.ectopicBeat;
    const widthScale = ectopic ? 2.2 : this.params.qrsWidthScale;
    const p = bump(u, P_CENTER, 0.03, ectopic ? 0 : 0.15);
    const q = -bump(u, Q_CENTER, 0.0096 * widthScale, 0.15);
    const r = bump(u, R_CENTER, 0.012 * widthScale, ectopic ? 1.5 : 1.0);
    const s = -bump(u, S_CENTER + 0.012 * (widthScale - 1), 0.012 * widthScale, 0.25);
    const t = bump(u, T_CENTER, 0.054, ectopic ? -0.4 : 0.3);
    const st = ectopic ? 0 : bump(u, ST_CENTER, 0.06, this.params.stElevation);
    return p + q + r + s + t + st;
  }

  /** Slow respiratory baseline wander. */
  private baseline(): number {
    const t = this.elapsedSeconds;
    return 0.04 * Math.sin(2 * Math.PI * 0.28 * t) + 0.02 * Math.sin(2 * Math.PI * 0.11 * t);
  }
}
