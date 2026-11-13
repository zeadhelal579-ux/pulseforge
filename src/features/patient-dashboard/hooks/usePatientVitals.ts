import { useEffect, useState } from 'react';
import { useSignalR } from '@/shared/hooks/useSignalR';
import type {
  RiskStatusLevel,
  RiskStatusPayload,
  VitalsSample,
  VitalsTickPayload,
  RiskAssessment,
} from '@/shared/types/api';

export interface LiveVitalsState {
  heartRate: number | null;
  spo2: number | null;
  statusLevel: RiskStatusLevel | null;
  confidenceScore: number | null;
  riskScore?: number;
  flags: string[];
  lastUpdatedAt: string | null;
}

interface VitalsSeed {
  vitals: VitalsSample | null;
  riskAssessment: RiskAssessment | null;
}

function seedToState(seed: VitalsSeed): LiveVitalsState {
  return {
    heartRate: seed.vitals?.heartRate ?? null,
    spo2: seed.vitals?.spo2 ?? null,
    statusLevel: seed.riskAssessment?.statusLevel ?? null,
    confidenceScore: seed.riskAssessment?.confidenceScore ?? null,
    riskScore: seed.riskAssessment?.riskScore,
    flags: seed.riskAssessment?.flags ?? [],
    lastUpdatedAt: seed.vitals?.timestamp ?? null,
  };
}

/**
 * Combines the initial REST-fetched values (from the patient profile
 * aggregate) with live `vitals:{patientId}` and `riskStatus:{patientId}`
 * SignalR events. Unlike the raw ECG stream, HR/SpO2/risk ticks arrive at a
 * low enough rate (roughly once per reading window, not 250/sec) that
 * updating them via normal React state is the right tool — no ring buffer
 * needed here.
 */
export function usePatientVitals(patientId: string | undefined, seed: VitalsSeed): LiveVitalsState {
  const { connection, connectionState } = useSignalR();
  const [state, setState] = useState<LiveVitalsState>(() => seedToState(seed));

  // Re-seed whenever the underlying REST data changes (patient switched,
  // manual refresh, background refetch) — depends on the specific fields
  // rather than the whole `seed` object so a fresh object literal from the
  // caller on every render doesn't retrigger this unnecessarily.
  useEffect(() => {
    setState(seedToState(seed));
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [seed.vitals, seed.riskAssessment]);

  useEffect(() => {
    if (!patientId || !connection || connectionState !== 'connected') return;

    const handleVitalsTick = (payload: VitalsTickPayload) => {
      setState((prev) => ({
        ...prev,
        heartRate: payload.heartRate,
        spo2: payload.spo2,
        lastUpdatedAt: payload.timestamp,
      }));
    };

    const handleRiskStatus = (payload: RiskStatusPayload) => {
      setState((prev) => ({
        ...prev,
        statusLevel: payload.statusLevel,
        confidenceScore: payload.confidenceScore,
        riskScore: payload.riskScore,
        flags: payload.flags,
      }));
    };

    connection.on(`vitals:${patientId}`, handleVitalsTick);
    connection.on(`riskStatus:${patientId}`, handleRiskStatus);

    return () => {
      connection.off(`vitals:${patientId}`, handleVitalsTick);
      connection.off(`riskStatus:${patientId}`, handleRiskStatus);
    };
  }, [connection, connectionState, patientId]);

  return state;
}
