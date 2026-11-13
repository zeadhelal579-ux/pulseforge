/** Matches the `?range=` query param on GET /patients/{id|me}/vitals-history. */
export type VitalsHistoryRange = '24h' | '7d' | '30d';

/** Connection state of the live ECG/vitals SignalR subscription for one patient. */
export type LiveStreamStatus = 'connecting' | 'live' | 'idle' | 'error';
