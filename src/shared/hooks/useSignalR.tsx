import { createContext, useContext } from 'react';
import type { HubConnection } from '@microsoft/signalr';

export type SignalRConnectionState = 'connecting' | 'connected' | 'reconnecting' | 'disconnected';

export interface SignalRContextValue {
  connection: HubConnection | null;
  connectionState: SignalRConnectionState;
}

/**
 * Default value covers the (normal) case of rendering before SignalRProvider
 * has established a connection yet — consumers treat `connection: null` as
 * "not connected", not as a bug.
 */
export const SignalRContext = createContext<SignalRContextValue>({
  connection: null,
  connectionState: 'disconnected',
});

export function useSignalR(): SignalRContextValue {
  return useContext(SignalRContext);
}
