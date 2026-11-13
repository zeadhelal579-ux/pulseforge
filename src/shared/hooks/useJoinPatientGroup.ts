import { useEffect } from 'react';
import { useSignalR } from '@/shared/hooks/useSignalR';

/**
 * Not used by the patient's own dashboard — a patient's connection joins
 * their own group automatically server-side from their JWT claim on connect
 * (backend-database-architecture.md §5), so calling this for a patient would
 * be an unnecessary/likely-rejected hub call. Only the doctor's
 * PatientDetailPage should use this.
 */
export function useJoinPatientGroup(patientId: string | undefined, enabled: boolean): void {
  const { connection, connectionState } = useSignalR();

  useEffect(() => {
    if (!enabled || !patientId || !connection || connectionState !== 'connected') return;

    connection.invoke('JoinPatientGroup', patientId).catch(() => {
      console.error('Failed to join monitoring group');
    });

    return () => {
      connection.invoke('LeavePatientGroup', patientId).catch(() => {
        // Best-effort — the connection may already be closing on unmount/navigation.
      });
    };
  }, [connection, connectionState, patientId, enabled]);
}
