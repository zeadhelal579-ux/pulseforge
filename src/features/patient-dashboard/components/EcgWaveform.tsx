import { useEffect, useRef } from 'react';
import { useTranslation } from 'react-i18next';
import { Radio, Snowflake, Play, FileClock, PenLine } from 'lucide-react';
import { Card, CardHeader } from '@/shared/components/ui/Card';
import { Button } from '@/shared/components/ui/Button';
import { formatMs } from '@/shared/utils/formatters';
import { useLiveEcgStream } from '@/features/patient-dashboard/hooks/useLiveEcgStream';
import {
  ECG_CALIBRATION,
  ECG_GRID_MAJOR_EVERY,
  ECG_GRID_MINOR_PX,
  ECG_SAMPLE_RATE_HZ,
  ECG_WINDOW_SECONDS,
  ECG_WINDOW_SIZE,
} from '@/features/patient-dashboard/constants';
import {
  computeIdleLandmarkTimes,
  detectBeatLandmarks,
  generateIdleWindow,
  unwrapRingBuffer,
  type BeatLandmarkIndices,
} from '@/features/patient-dashboard/components/ecgWaveformMath';
import type { EcgFeatureSet } from '@/shared/types/api';

interface EcgWaveformProps {
  patientId: string;
  /** Real backend-computed interval numbers shown as text — never derived
   *  from the canvas math above, which is visual-only (see ecgWaveformMath.ts). */
  ecgFeatureSet: EcgFeatureSet | null;
  isFrozen?: boolean;
  /** Doctor-only controls — omitted entirely on the patient's own view. */
  onToggleFreeze?: () => void;
  onReviewStrip?: () => void;
  onLogManualEvent?: () => void;
  className?: string;
}

interface CanvasColors {
  gridMinor: string;
  gridMajor: string;
  trace: string;
  labelText: string;
  labelBg: string;
}

/** Canvas can't resolve `var(--color-x)` itself — read the resolved values
 *  from computed style each frame so light/dark theme switches apply live. */
function readCanvasColors(canvas: HTMLCanvasElement): CanvasColors {
  const styles = getComputedStyle(canvas);
  const read = (name: string) => styles.getPropertyValue(name).trim();
  return {
    gridMinor: read('--color-border'),
    gridMajor: read('--color-border-strong'),
    trace: read('--color-accent'),
    labelText: read('--color-text-primary'),
    labelBg: read('--color-surface'),
  };
}

function drawGrid(ctx: CanvasRenderingContext2D, width: number, height: number, colors: CanvasColors) {
  const minor = ECG_GRID_MINOR_PX;

  ctx.lineWidth = 1;
  for (let x = 0; x <= width; x += minor) {
    const isMajor = Math.round(x / minor) % ECG_GRID_MAJOR_EVERY === 0;
    ctx.strokeStyle = isMajor ? colors.gridMajor : colors.gridMinor;
    ctx.globalAlpha = isMajor ? 0.55 : 0.25;
    ctx.beginPath();
    ctx.moveTo(x, 0);
    ctx.lineTo(x, height);
    ctx.stroke();
  }
  for (let y = 0; y <= height; y += minor) {
    const isMajor = Math.round(y / minor) % ECG_GRID_MAJOR_EVERY === 0;
    ctx.strokeStyle = isMajor ? colors.gridMajor : colors.gridMinor;
    ctx.globalAlpha = isMajor ? 0.55 : 0.25;
    ctx.beginPath();
    ctx.moveTo(0, y);
    ctx.lineTo(width, y);
    ctx.stroke();
  }
  ctx.globalAlpha = 1;
}

/** Maps a sample value to a canvas y-coordinate, auto-scaled to the visible
 *  window's own min/max — this is what keeps the trace readable regardless
 *  of whatever raw unit/amplitude the backend ends up sending. */
function buildAmplitudeMapper(samples: Float32Array, height: number) {
  let min = Infinity;
  let max = -Infinity;
  for (let i = 0; i < samples.length; i += 1) {
    const value = samples[i] ?? 0;
    if (value < min) min = value;
    if (value > max) max = value;
  }
  if (!Number.isFinite(min) || !Number.isFinite(max) || max - min < 1e-6) {
    min = -1;
    max = 1;
  }
  const padding = height * 0.12;
  const usableHeight = height - padding * 2;
  const range = max - min;

  return (value: number) => padding + ((max - value) / range) * usableHeight;
}

function drawWaveform(
  ctx: CanvasRenderingContext2D,
  samples: Float32Array,
  width: number,
  height: number,
  colors: CanvasColors,
): (index: number) => number {
  const toY = buildAmplitudeMapper(samples, height);
  const pxPerSample = width / samples.length;

  ctx.strokeStyle = colors.trace;
  ctx.lineWidth = 2;
  ctx.lineJoin = 'round';
  ctx.beginPath();
  for (let i = 0; i < samples.length; i += 1) {
    const x = i * pxPerSample;
    const y = toY(samples[i] ?? 0);
    if (i === 0) ctx.moveTo(x, y);
    else ctx.lineTo(x, y);
  }
  ctx.stroke();

  // Live "pen" dot at the most recent sample.
  const lastX = (samples.length - 1) * pxPerSample;
  const lastY = toY(samples[samples.length - 1] ?? 0);
  ctx.fillStyle = colors.trace;
  ctx.beginPath();
  ctx.arc(lastX, lastY, 3.5, 0, Math.PI * 2);
  ctx.fill();

  return toY;
}

const LANDMARK_VERTICAL_OFFSET: Record<keyof BeatLandmarkIndices, number> = {
  p: -16,
  q: 14,
  r: -20,
  s: 14,
  t: -16,
};

function drawLandmarkLabels(
  ctx: CanvasRenderingContext2D,
  landmarks: BeatLandmarkIndices,
  samples: Float32Array,
  width: number,
  toY: (index: number) => number,
  colors: CanvasColors,
) {
  const pxPerSample = width / samples.length;
  const letters: Array<keyof BeatLandmarkIndices> = ['p', 'q', 'r', 's', 't'];

  ctx.font = '600 10px var(--font-mono, monospace)';
  ctx.textAlign = 'center';
  ctx.textBaseline = 'middle';

  for (const key of letters) {
    const index = landmarks[key];
    if (index < 0 || index >= samples.length) continue;

    const x = index * pxPerSample;
    const pointY = toY(samples[index] ?? 0);
    const labelY = pointY + LANDMARK_VERTICAL_OFFSET[key];

    // Marker dot on the trace itself.
    ctx.fillStyle = colors.trace;
    ctx.beginPath();
    ctx.arc(x, pointY, 2, 0, Math.PI * 2);
    ctx.fill();

    // Small pill background so the letter stays legible over the grid/trace.
    ctx.fillStyle = colors.labelBg;
    ctx.globalAlpha = 0.85;
    ctx.beginPath();
    ctx.roundRect(x - 7, labelY - 7, 14, 14, 4);
    ctx.fill();
    ctx.globalAlpha = 1;

    ctx.fillStyle = colors.labelText;
    ctx.fillText(key.toUpperCase(), x, labelY + 0.5);
  }
}

export function EcgWaveform({
  patientId,
  ecgFeatureSet,
  isFrozen = false,
  onToggleFreeze,
  onReviewStrip,
  onLogManualEvent,
  className,
}: EcgWaveformProps) {
  const { t, i18n } = useTranslation();
  const { bufferRef, writeIndexRef, status } = useLiveEcgStream(patientId);

  const containerRef = useRef<HTMLDivElement>(null);
  const canvasRef = useRef<HTMLCanvasElement>(null);
  const sizeRef = useRef({ width: 0, height: 0 });
  const isFrozenRef = useRef(isFrozen);
  const statusRef = useRef(status);
  const rafIdRef = useRef<number | null>(null);
  // Cached separately from the draw loop below: colors only ever change when
  // the theme toggles (ThemeToggle flips <html data-theme>), so recomputing
  // them via getComputedStyle() on every single animation frame — up to 60
  // times a second, per mounted waveform — would undermine the exact
  // "keep the live-data path off the render/style-recalc hot path"
  // performance goal this component otherwise goes out of its way for.
  const colorsRef = useRef<CanvasColors | null>(null);

  useEffect(() => {
    isFrozenRef.current = isFrozen;
  }, [isFrozen]);
  useEffect(() => {
    statusRef.current = status;
  }, [status]);

  // Read the theme-dependent canvas colors once up front, then only again
  // when <html data-theme> actually changes — not on every draw-loop frame.
  useEffect(() => {
    const canvas = canvasRef.current;
    if (!canvas) return;

    colorsRef.current = readCanvasColors(canvas);
    const observer = new MutationObserver(() => {
      colorsRef.current = readCanvasColors(canvas);
    });
    observer.observe(document.documentElement, {
      attributes: true,
      attributeFilter: ['data-theme'],
    });
    return () => observer.disconnect();
  }, []);

  // Keep the canvas's backing store matched to its displayed CSS size and to
  // devicePixelRatio, so the trace stays crisp on high-DPI screens.
  useEffect(() => {
    const canvas = canvasRef.current;
    const container = containerRef.current;
    if (!canvas || !container) return;

    const handleResize = () => {
      const rect = container.getBoundingClientRect();
      const dpr = window.devicePixelRatio || 1;
      canvas.width = Math.max(1, Math.round(rect.width * dpr));
      canvas.height = Math.max(1, Math.round(rect.height * dpr));
      const ctx = canvas.getContext('2d');
      ctx?.setTransform(dpr, 0, 0, dpr, 0, 0);
      sizeRef.current = { width: rect.width, height: rect.height };
    };

    handleResize();
    const observer = new ResizeObserver(handleResize);
    observer.observe(container);
    return () => observer.disconnect();
  }, []);

  // The draw loop itself is set up once and never torn down/rebuilt on
  // re-render — it reads isFrozen/status via refs instead of closing over
  // props, which is what makes this safe to run at 60fps without React
  // re-rendering the component on every frame.
  useEffect(() => {
    const canvas = canvasRef.current;
    const ctx = canvas?.getContext('2d');
    if (!canvas || !ctx) return;

    const draw = () => {
      rafIdRef.current = requestAnimationFrame(draw);
      if (isFrozenRef.current) return; // paused — leave the last frame on screen

      const { width, height } = sizeRef.current;
      if (width === 0 || height === 0) return;

      // Fall back to a direct read only if the mount-time effect above
      // hasn't populated the cache yet (should be at most the very first
      // frame) — draw() must never assume colorsRef.current is non-null.
      const colors = colorsRef.current ?? readCanvasColors(canvas);
      ctx.clearRect(0, 0, width, height);
      drawGrid(ctx, width, height, colors);

      const isLive = statusRef.current === 'live';
      let samples: Float32Array;
      let landmarks: BeatLandmarkIndices | null;

      if (isLive) {
        samples = unwrapRingBuffer(bufferRef.current, writeIndexRef.current);
        landmarks = detectBeatLandmarks(samples, ECG_SAMPLE_RATE_HZ);
      } else {
        const now = performance.now() / 1000;
        samples = generateIdleWindow(now, ECG_WINDOW_SIZE, ECG_SAMPLE_RATE_HZ);
        const landmarkTimes = computeIdleLandmarkTimes(now);
        const windowStart = now - ECG_WINDOW_SECONDS;
        const timeToIndex = (time: number) =>
          Math.round(((time - windowStart) / ECG_WINDOW_SECONDS) * ECG_WINDOW_SIZE);
        landmarks = {
          p: timeToIndex(landmarkTimes.p),
          q: timeToIndex(landmarkTimes.q),
          r: timeToIndex(landmarkTimes.r),
          s: timeToIndex(landmarkTimes.s),
          t: timeToIndex(landmarkTimes.t),
        };
      }

      const toY = drawWaveform(ctx, samples, width, height, colors);
      if (landmarks) drawLandmarkLabels(ctx, landmarks, samples, width, toY, colors);
    };

    rafIdRef.current = requestAnimationFrame(draw);
    return () => {
      if (rafIdRef.current !== null) cancelAnimationFrame(rafIdRef.current);
    };
  }, [bufferRef, writeIndexRef]);

  return (
    <Card className={className}>
      <CardHeader className="flex-wrap">
        <div className="flex items-center gap-2">
          <span className="rounded-md bg-surface-alt px-2 py-1 font-mono text-[11px] font-bold text-text-secondary">
            {t('patient.ecg.leadII')}
          </span>
          {status === 'live' && (
            <span className="flex items-center gap-1 text-[11px] font-semibold text-accent">
              <Radio className="size-3 animate-pulse" />
              {t('patient.ecg.live')}
            </span>
          )}
        </div>
        <div className="flex flex-wrap items-center gap-2">
          {onReviewStrip && (
            <Button variant="secondary" size="sm" onClick={onReviewStrip}>
              <FileClock className="size-3.5" />
              {t('patient.ecg.reviewStrip')}
            </Button>
          )}
          {onToggleFreeze && (
            <Button variant="secondary" size="sm" onClick={onToggleFreeze}>
              {isFrozen ? <Play className="size-3.5" /> : <Snowflake className="size-3.5" />}
              {isFrozen ? t('patient.ecg.unfreezeWaveform') : t('patient.ecg.freezeWaveform')}
            </Button>
          )}
          {onLogManualEvent && (
            <Button variant="secondary" size="sm" onClick={onLogManualEvent}>
              <PenLine className="size-3.5" />
              {t('patient.ecg.logManualEvent')}
            </Button>
          )}
        </div>
      </CardHeader>

      <div ref={containerRef} className="relative h-[220px] w-full overflow-hidden rounded-lg bg-surface-alt">
        <canvas ref={canvasRef} className="absolute inset-0 size-full" />
        {status !== 'live' && (
          <div className="absolute inset-x-0 top-2 mx-auto w-fit rounded-full bg-bg/70 px-3 py-1 text-[11px] text-text-muted backdrop-blur-sm">
            {t('patient.ecg.awaitingSignal')}
          </div>
        )}
      </div>

      <div className="mt-3 flex flex-wrap items-center gap-x-5 gap-y-1 font-mono text-[11px] text-text-muted">
        <span>
          {t('patient.ecg.calibration')}: {ECG_CALIBRATION.paperSpeedMmPerSec} mm/s
        </span>
        <span>
          {t('patient.ecg.gain')}: {ECG_CALIBRATION.gainMmPerMv} mm/mV
        </span>
        <span>
          {t('patient.ecg.sampleRate')}: {ECG_CALIBRATION.sampleRateHz} Hz
        </span>
        {ecgFeatureSet && (
          <>
            <span>
              {t('patient.ecg.prInterval')}: {formatMs(ecgFeatureSet.prIntervalMs, i18n.language as 'ar' | 'en')}
            </span>
            <span>
              {t('patient.ecg.qrsDuration')}:{' '}
              {formatMs(ecgFeatureSet.qrsDurationMs, i18n.language as 'ar' | 'en')}
            </span>
            <span>
              {t('patient.ecg.qtQtc')}: {ecgFeatureSet.qtQtcMs[0]}/{ecgFeatureSet.qtQtcMs[1]} ms
            </span>
          </>
        )}
      </div>
    </Card>
  );
}
