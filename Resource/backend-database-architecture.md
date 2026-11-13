# Backend and database architecture

`back-end/back/cardiosense-backend` is the .NET solution. [Architecture overview](architecture-overview.md) owns API paths and access. [File map](architecture-file-map.md) locates each source file.

## Layers

Domain owns entities, value constraints and state transitions. Application owns commands, queries, validators, permissions and service interfaces. Infrastructure implements EF Core, SQL queries, Identity, MQTT, SignalR and HTTP inference. Api owns endpoint binding, middleware, policies, CORS and dependency injection. The hub lives in Infrastructure so the notifier can reference it. API configuration applies the authenticated hub filter and maps the route.

## Operation design

| API group | Processing and storage |
|---|---|
| 01–05 Auth | Validate input, create or verify Identity account, issue access token and hash refresh token. Rotate and revoke through RefreshTokens. Defaults are 15 minutes access and 7 days refresh |
| 06–07 and 11 Profiles | Validate caller and ownership. Assemble data from patient, device, clinical history, vitals and risk. Apply role-specific DTO. Updates only touch supported fields |
| 08–09 and 12–13 History and alerts | Enforce ownership, resolve range, fetch by PatientId and timestamp. Return durable history |
| 10 and 14 Reports | Resolve period, aggregate SQL readings, invoke FastAPI and project permitted fields. Missing data yields 404 |
| 15–17 Clinical actions | Verify assigned doctor and alert ownership. Log action or update alert state. Persist changes before successful response |
| 18–22 Doctor | Load doctor profile, create assignment or query assigned patients and statistics. Enforce uniqueness and capacity policies |
| 23–24 Devices | Check inventory and caller ownership, bind device or return status. Duplicate pairing can yield 409 |

Telemetry parsing and feature processing are entered through the MQTT background worker. Vitals, features and assessment writes happen in stages. Assessment errors do not delete prior writes. The live notification is best effort. Before multiple workers or device retries, define durable message IDs and enforce uniqueness and transition ordering.

## SQL catalog

The [SQL schema reference](sql-schema.md) lists columns, SQL types, nullability and keys for all 16 tables. [InitialCreate](https://github.com/zeadhelal579-ux/pulseforge/blob/back-end/cardiosense-backend/src/CardioSense.Infrastructure/Persistence/Migrations/20261113045259_InitialCreate.cs) defines FK delete behavior and index predicates.

| Table | Identity and relations | Responsibility |
|---|---|---|
| Users | Id, normalized identity indexes | Account hash, lockout, login |
| UserClaims | Id and UserId | ASP.NET Identity claims |
| UserLogins | LoginProvider and ProviderKey, UserId | ASP.NET Identity external login metadata |
| UserTokens | UserId, LoginProvider, Name | ASP.NET Identity tokens |
| RefreshTokens | Id and UserId, unique TokenHash | Rotation, expiry, revocation and reuse handling |
| Patients | Id and UserId, unique NationalId and FileNumber | Profile and patient ownership |
| Doctors | Id and UserId, unique LicenseNumber | Clinician profile |
| DoctorPatientAssignments | Id, DoctorId and PatientId | Unique pair and filtered primary assignment index |
| Devices | Id, SerialNumber and optional unique PatientId | Device inventory, pair, battery, connection and last seen |
| MedicalHistoryEntries | Id and PatientId | Clinical entries and inference context |
| VitalsSamples | Id, PatientId, DeviceId, Timestamp | HR and oxygen, indexed history |
| EcgFeatureSets | Id, PatientId, Timestamp | PR, QRS, QT, QTc and feature JSON |
| ElectrodeVerifications | Id, PatientId, Timestamp | Verification evidence when available |
| RiskAssessments | Id, PatientId, EcgFeatureSetId, Timestamp | Version, status, score, confidence and flags |
| Alerts | Id, PatientId, TriggeredAt, ResolvedAt | Automated and manual alerts |
| AuditLogs | Id, UserId, EntityId, Timestamp | Access and action audit |

`PatientFileNumbers` allocates file numbers. EF records applied migrations in `__EFMigrationsHistory`. A database created using EnsureCreated needs a reviewed baseline or migration to a new database. See [EF Core guidance](https://learn.microsoft.com/en-us/ef/core/managing-schemas/migrations/applying).

## Testing and open decisions

Run Application and Infrastructure suites, then the SQL-backed API integration suite. Test simultaneous assignment and pairing, data retention and restore, out-of-order readings and Critical resolution before a pilot. Scale time-series storage only when query and capacity evidence supports it. [Deployment](infrastructure-deployment.md).
