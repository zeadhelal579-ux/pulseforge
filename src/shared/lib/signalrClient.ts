import { HubConnectionBuilder, LogLevel, type HubConnection } from '@microsoft/signalr';
import { createDemoHubConnection } from '@/shared/demo/demoHub';
import { IS_DEMO_MODE, SIGNALR_HUB_URL } from '@/shared/lib/env';

/**
 * Builds (but does not start) a HubConnection pointed at the monitoring hub
 * (backend-database-architecture.md §5 "SignalR hub — /hubs/monitoring").
 *
 * `accessTokenFactory` is re-invoked by the SignalR client on every reconnect
 * attempt, so passing a function (not a static token string) means a token
 * refreshed mid-session is picked up automatically without recreating the
 * connection.
 */
export function createHubConnection(accessTokenFactory: () => string | null): HubConnection {
  if (IS_DEMO_MODE) {
    // The demo hub implements the exact subset of HubConnection the app calls (on, off, invoke, start, stop,
    // onreconnecting, onreconnected, onclose). The cast is needed because HubConnection has private members,
    // which makes it impossible to satisfy structurally from another class.
    return createDemoHubConnection(accessTokenFactory) as unknown as HubConnection;
  }

  return new HubConnectionBuilder()
    .withUrl(SIGNALR_HUB_URL, {
      accessTokenFactory: () => accessTokenFactory() ?? '',
    })
    .withAutomaticReconnect([0, 2000, 5000, 10000, 20000]) // backoff schedule, in ms
    .configureLogging(import.meta.env.DEV ? LogLevel.Information : LogLevel.Warning)
    .build();
}
