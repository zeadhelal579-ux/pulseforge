# Infrastructure and deployment

| Environment | Runtime | Evidence |
|---|---|---|
| Local | Vite 5173, ASP.NET 5001 HTTPS and 5000 HTTP, FastAPI 8000, LocalDB, test MQTT 18883 | Synthetic browser and SQL integration tests |
| Compose development | API, SQL Server, TLS Mosquitto, private FastAPI, optional Redis profile | Configuration present. Full Compose run remains unverified |
| Vercel snapshot | Separate `front Deploy` project, demo or configured API mode | Snapshot scope. No documented synchronization with local source |
| Pilot target | HTTPS edge, private API dependencies and authorized TLS broker | Pending device and operations gates |

Vite proxies /api and /hubs locally. Prefer one public browser origin for static assets, REST and WebSockets. For split origins validate the exact CORS allowlist, Origin guard and refresh cookie behavior. Never place secrets in public VITE settings. Hide token-bearing URLs from server and proxy logs. See [SignalR security](https://learn.microsoft.com/en-us/aspnet/core/signalr/security?view=aspnetcore-10.0).

## Configuration ownership

| Area | Settings |
|---|---|
| Browser | VITE_API_BASE_URL, VITE_SIGNALR_HUB_URL, VITE_DEFAULT_LANGUAGE |
| SQL and identity | ConnectionStrings__DefaultConnection, Database__Initialization, Jwt__SigningKey, Jwt__Issuer, Jwt__Audience, Jwt__AccessTokenMinutes, Jwt__RefreshTokenDays |
| Browser security | Cors__AllowedOrigins__0, RefreshCookie__Name, RefreshCookie__Path, RefreshCookie__SameSite, RateLimiting__AuthPermitLimit, RateLimiting__AuthWindowSeconds |
| MQTT | Mqtt__Enabled, Mqtt__BrokerHost, Mqtt__BrokerPort, Mqtt__UseTls, Mqtt__Username, Mqtt__Password, Mqtt__ClientId, Mqtt__ReconnectDelaySeconds |
| Assessment and monitoring | AiService__BaseUrl, AiService__TimeoutSeconds, Monitoring__DoctorPatientCapacity, Monitoring__DeviceLiveWindowSeconds |
| EF and tests | CARDIOSENSE_CONNECTION_STRING for tooling, CARDIOSENSE_TEST_SQL for dedicated SQL integration |
| Compose | Use the checked-in .env.example keys for SQL, MQTT and JWT. Use unique secrets per environment |

## Runbook and release gates

1. Provision private SQL and apply a reviewed migration. Verify /health/ready and restore a backup to a disposable database.
2. Start private FastAPI and verify /health. Provision MQTT device accounts and ACLs with TLS. Verify read and write scopes.
3. Start API and frontend. Rehearse synthetic signup, pairing, assignment, telemetry, history and alert lifecycle.
4. Record p95 ingestion latency, last sample age, error counts and storage growth. Agree on SLO, RPO and RTO based on pilot scale.
5. Exercise rollback, SQL schema compatibility, broker outage and AI degradation. Restrict public exposure until the pilot owner signs off.

[Audit](../audit-report/REPORT.md) records the actual run. Data residency, retention, certificate management and on-call responsibility remain open.
