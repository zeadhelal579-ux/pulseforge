import { useEffect, useRef, useState } from 'react';
import { useSignalR } from '@/shared/hooks/useSignalR';
import { ECG_WINDOW_SIZE } from '@/features/patient-dashboard/constants';
import type { LiveStreamStatus } from '@/features/patient-dashboard/types';

/** A plain mutable ref that's always initialized — deliberately not the
 *  nullable `RefObject<T>` shape React uses for possibly-unattached DOM
 *  refs, since bufferRef/writeIndexRef below are "value refs" seeded with a
 *  real value up front and are never null in practice. */
interface ValueRef<T> {
  current: T;
}

export interface LiveEcgStream {
  /** Ring buffer of raw samples — read this inside a rAF loop, never via render. */
  bufferRef: ValueRef<Float32Array>;
  /** Index the *next* sample will be written to (i.e. one past the most recent). */
  writeIndexRef: ValueRef<number>;
  status: LiveStreamStatus;
}

/**
 * Subscribes to the `ecg:{patientId}` SignalR event (backend-database-
 * architecture.md §5) and writes each incoming sample straight into a ring
 * buffer ref. This intentionally does NOT trigger a re-render per sample —
 * EcgWaveform.tsx reads the buffer directly inside its own rAF draw loop.
 *
 * `status` is the one piece of state that *is* allowed to trigger a
 * re-render here, and only flips once (idle → live) the first time a real
 * sample arrives — not on every sample — so the connection-status dot can
 * update without reintroducing the per-sample re-render problem.
 */
export function useLiveEcgStream(patientId: string | undefined): LiveEcgStream {
  const bufferRef = useRef<Float32Array>(new Float32Array(ECG_WINDOW_SIZE));
  const writeIndexRef = useRef(0);
  const hasReceivedSampleRef = useRef(false);
  const lastPatientIdRef = useRef<string | undefined>(undefined);
  const [status, setStatus] = useState<LiveStreamStatus>('connecting');

  const { connection, connectionState } = useSignalR();

  useEffect(() => {
    hasReceivedSampleRef.current = false;

    // Defense in depth against a stale-data cross-patient bleed: if the
    // *caller* ever reuses this hook's component instance across two
    // different patients without remounting it (e.g. PatientDetailPage's
    // <PatientDetailChrome key={patientId}> fix is bypassed, or this hook
    // is reused somewhere new in the future), the ring buffer must never be
    // allowed to keep showing a previously-viewed patient's leftover
    // samples — a clinician could otherwise briefly mistake one patient's
    // cardiac waveform for another's.
    if (patientId !== lastPatientIdRef.current) {
      bufferRef.current.fill(0);
      writeIndexRef.current = 0;
      lastPatientIdRef.current = patientId;
    }

    if (!patientId) {
      setStatus('idle');
      return;
    }
    if (connectionState !== 'connected' || !connection) {
      setStatus(connectionState === 'reconnecting' ? 'connecting' : 'connecting');
      return;
    }

    setStatus('idle'); // connected, but no sample has arrived yet
    const eventName = `ecg:${patientId}`;

    const handleSample = (sample: number) => {
      bufferRef.current[writeIndexRef.current] = sample;
      writeIndexRef.current = (writeIndexRef.current + 1) % ECG_WINDOW_SIZE;

      if (!hasReceivedSampleRef.current) {
        hasReceivedSampleRef.current = true;
        setStatus('live'); // fires exactly once per (re)subscription, not per sample
      }
    };

    connection.on(eventName, handleSample);
    return () => connection.off(eventName, handleSample);
  }, [connection, connectionState, patientId]);

  return { bufferRef, writeIndexRef, status };
}
