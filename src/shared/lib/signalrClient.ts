import { HubConnectionBuilder, LogLevel, type HubConnection } from '@microsoft/signalr';

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
  const hubUrl = import.meta.env.VITE_SIGNALR_HUB_URL ?? '/hubs/monitoring';

  return new HubConnectionBuilder()
    .withUrl(hubUrl, {
      accessTokenFactory: () => accessTokenFactory() ?? '',
    })
    .withAutomaticReconnect([0, 2000, 5000, 10000, 20000]) // backoff schedule, in ms
    .configureLogging(import.meta.env.DEV ? LogLevel.Information : LogLevel.Warning)
    .build();
}
