import { useEffect, useRef, useState, type ReactNode } from 'react';
import type { HubConnection } from '@microsoft/signalr';
import { createHubConnection } from '@/shared/lib/signalrClient';
import { SignalRContext, type SignalRConnectionState } from '@/shared/hooks/useSignalR';
import { useAuthStore } from '@/features/auth/store/authStore';

interface SignalRProviderProps {
  children: ReactNode;
}

/**
 * Composition-root provider: this is the one place allowed to reach into both
 * shared/lib (the connection factory) and features/auth (the access token) —
 * see the layering note in shared/hooks/useSignalR.tsx for why the context
 * itself lives in shared/ instead of here.
 */
export function SignalRProvider({ children }: SignalRProviderProps) {
  const accessToken = useAuthStore((state) => state.accessToken);
  const status = useAuthStore((state) => state.status);
  const connectionRef = useRef<HubConnection | null>(null);
  const [connectionState, setConnectionState] = useState<SignalRConnectionState>('disconnected');
  const [connection, setConnection] = useState<HubConnection | null>(null);

  useEffect(() => {
    // Only hold a live socket while actually authenticated — an anonymous
    // visitor on the auth page has no per-patient data to subscribe to.
    if (status !== 'authenticated' || !accessToken) {
      connectionRef.current?.stop();
      connectionRef.current = null;
      setConnection(null);
      setConnectionState('disconnected');
      return;
    }

    const hubConnection = createHubConnection(() => useAuthStore.getState().accessToken);
    connectionRef.current = hubConnection;

    hubConnection.onreconnecting(() => setConnectionState('reconnecting'));
    hubConnection.onreconnected(() => setConnectionState('connected'));
    hubConnection.onclose(() => setConnectionState('disconnected'));

    setConnectionState('connecting');
    hubConnection
      .start()
      .then(() => {
        setConnectionState('connected');
        setConnection(hubConnection);
      })
      .catch((error) => {
        console.error('SignalR connection failed:', error);
        setConnectionState('disconnected');
      });

    return () => {
      hubConnection.stop();
      if (connectionRef.current === hubConnection) connectionRef.current = null;
    };
    // Re-run only when the *authentication* state changes, not on every token
    // refresh — the connection's accessTokenFactory closure above always
    // reads the latest token from the store already.
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [status]);

  return (
    <SignalRContext.Provider value={{ connection, connectionState }}>
      {children}
    </SignalRContext.Provider>
  );
}
