# SQL schema reference

Generated from the InitialCreate migration. The migration remains the executable authority. See [backend and database architecture](backend-database-architecture.md) for ownership and write paths.

## Users

| Column | SQL type | Nullable |
|---|---|---|
| `Id` | `uniqueidentifier` | No |
| `Role` | `nvarchar(32)` | No |
| `PreferredLanguage` | `nvarchar(10)` | No |
| `CreatedAt` | `datetimeoffset` | No |
| `UserName` | `nvarchar(256)` | Yes |
| `NormalizedUserName` | `nvarchar(256)` | Yes |
| `Email` | `nvarchar(256)` | Yes |
| `NormalizedEmail` | `nvarchar(256)` | Yes |
| `EmailConfirmed` | `bit` | No |
| `PasswordHash` | `nvarchar(max)` | Yes |
| `SecurityStamp` | `nvarchar(max)` | Yes |
| `ConcurrencyStamp` | `nvarchar(max)` | Yes |
| `PhoneNumber` | `nvarchar(max)` | Yes |
| `PhoneNumberConfirmed` | `bit` | No |
| `TwoFactorEnabled` | `bit` | No |
| `LockoutEnd` | `datetimeoffset` | Yes |
| `LockoutEnabled` | `bit` | No |
| `AccessFailedCount` | `int` | No |

- PrimaryKey: `PK_Users`

## AuditLogs

| Column | SQL type | Nullable |
|---|---|---|
| `Id` | `uniqueidentifier` | No |
| `UserId` | `uniqueidentifier` | Yes |
| `Action` | `nvarchar(100)` | No |
| `EntityType` | `nvarchar(100)` | No |
| `EntityId` | `uniqueidentifier` | Yes |
| `Timestamp` | `datetimeoffset` | No |
| `IpAddress` | `nvarchar(64)` | Yes |
| `CreatedAt` | `datetimeoffset` | No |

- PrimaryKey: `PK_AuditLogs`
- ForeignKey: `FK_AuditLogs_Users_UserId`

## Doctors

| Column | SQL type | Nullable |
|---|---|---|
| `Id` | `uniqueidentifier` | No |
| `UserId` | `uniqueidentifier` | No |
| `FullName` | `nvarchar(120)` | No |
| `Specialty` | `nvarchar(120)` | No |
| `LicenseNumber` | `nvarchar(60)` | No |
| `Hospital` | `nvarchar(160)` | No |
| `Department` | `nvarchar(120)` | Yes |
| `Phone` | `nvarchar(20)` | No |
| `CreatedAt` | `datetimeoffset` | No |

- PrimaryKey: `PK_Doctors`
- ForeignKey: `FK_Doctors_Users_UserId`

## Patients

| Column | SQL type | Nullable |
|---|---|---|
| `Id` | `uniqueidentifier` | No |
| `UserId` | `uniqueidentifier` | No |
| `FileNumber` | `int` | No |
| `FullName` | `nvarchar(120)` | No |
| `DateOfBirth` | `date` | No |
| `Gender` | `nvarchar(32)` | No |
| `NationalId` | `nvarchar(20)` | No |
| `Phone` | `nvarchar(20)` | No |
| `CreatedAt` | `datetimeoffset` | No |

- PrimaryKey: `PK_Patients`
- ForeignKey: `FK_Patients_Users_UserId`

## RefreshTokens

| Column | SQL type | Nullable |
|---|---|---|
| `Id` | `uniqueidentifier` | No |
| `UserId` | `uniqueidentifier` | No |
| `TokenHash` | `nvarchar(128)` | No |
| `ExpiresAt` | `datetimeoffset` | No |
| `RevokedAt` | `datetimeoffset` | Yes |
| `CreatedAt` | `datetimeoffset` | No |

- PrimaryKey: `PK_RefreshTokens`
- ForeignKey: `FK_RefreshTokens_Users_UserId`

## UserClaims

| Column | SQL type | Nullable |
|---|---|---|
| `Id` | `int` | No |
| `UserId` | `uniqueidentifier` | No |
| `ClaimType` | `nvarchar(max)` | Yes |
| `ClaimValue` | `nvarchar(max)` | Yes |

- PrimaryKey: `PK_UserClaims`
- ForeignKey: `FK_UserClaims_Users_UserId`

## UserLogins

| Column | SQL type | Nullable |
|---|---|---|
| `LoginProvider` | `nvarchar(450)` | No |
| `ProviderKey` | `nvarchar(450)` | No |
| `ProviderDisplayName` | `nvarchar(max)` | Yes |
| `UserId` | `uniqueidentifier` | No |

- PrimaryKey: `PK_UserLogins`

## UserTokens

| Column | SQL type | Nullable |
|---|---|---|
| `UserId` | `uniqueidentifier` | No |
| `LoginProvider` | `nvarchar(450)` | No |
| `Name` | `nvarchar(450)` | No |
| `Value` | `nvarchar(max)` | Yes |

- PrimaryKey: `PK_UserTokens`

## Alerts

| Column | SQL type | Nullable |
|---|---|---|
| `Id` | `uniqueidentifier` | No |
| `PatientId` | `uniqueidentifier` | No |
| `Type` | `nvarchar(32)` | No |
| `Severity` | `nvarchar(32)` | No |
| `Message` | `nvarchar(2000)` | No |
| `TriggeredAt` | `datetimeoffset` | No |
| `ResolvedAt` | `datetimeoffset` | Yes |
| `ResolutionNote` | `nvarchar(2000)` | Yes |
| `CreatedAt` | `datetimeoffset` | No |

- PrimaryKey: `PK_Alerts`
- ForeignKey: `FK_Alerts_Patients_PatientId`

## Devices

| Column | SQL type | Nullable |
|---|---|---|
| `Id` | `uniqueidentifier` | No |
| `SerialNumber` | `nvarchar(60)` | No |
| `PatientId` | `uniqueidentifier` | Yes |
| `PairedAt` | `datetimeoffset` | Yes |
| `FirmwareVersion` | `nvarchar(30)` | No |
| `BatteryLevel` | `int` | No |
| `LastSeenAt` | `datetimeoffset` | Yes |
| `ConnectionMode` | `nvarchar(32)` | No |
| `SignalStrengthDbm` | `int` | Yes |
| `CreatedAt` | `datetimeoffset` | No |

- PrimaryKey: `PK_Devices`
- ForeignKey: `FK_Devices_Patients_PatientId`

## DoctorPatientAssignments

| Column | SQL type | Nullable |
|---|---|---|
| `Id` | `uniqueidentifier` | No |
| `DoctorId` | `uniqueidentifier` | No |
| `PatientId` | `uniqueidentifier` | No |
| `AssignedAt` | `datetimeoffset` | No |
| `IsPrimary` | `bit` | No |
| `CreatedAt` | `datetimeoffset` | No |

- PrimaryKey: `PK_DoctorPatientAssignments`
- ForeignKey: `FK_DoctorPatientAssignments_Doctors_DoctorId`
- ForeignKey: `FK_DoctorPatientAssignments_Patients_PatientId`

## EcgFeatureSets

| Column | SQL type | Nullable |
|---|---|---|
| `Id` | `uniqueidentifier` | No |
| `PatientId` | `uniqueidentifier` | No |
| `Timestamp` | `datetimeoffset` | No |
| `PrIntervalMs` | `int` | No |
| `QrsDurationMs` | `int` | No |
| `QtMs` | `int` | No |
| `QtcMs` | `int` | No |
| `FeatureVectorJson` | `nvarchar(max)` | Yes |
| `CreatedAt` | `datetimeoffset` | No |

- PrimaryKey: `PK_EcgFeatureSets`
- ForeignKey: `FK_EcgFeatureSets_Patients_PatientId`

## ElectrodeVerifications

| Column | SQL type | Nullable |
|---|---|---|
| `Id` | `uniqueidentifier` | No |
| `PatientId` | `uniqueidentifier` | No |
| `Timestamp` | `datetimeoffset` | No |
| `PlacementStatus` | `nvarchar(32)` | No |
| `ConfidenceScore` | `float` | No |
| `CreatedAt` | `datetimeoffset` | No |

- PrimaryKey: `PK_ElectrodeVerifications`
- ForeignKey: `FK_ElectrodeVerifications_Patients_PatientId`

## MedicalHistoryEntries

| Column | SQL type | Nullable |
|---|---|---|
| `Id` | `uniqueidentifier` | No |
| `PatientId` | `uniqueidentifier` | No |
| `EntryType` | `nvarchar(32)` | No |
| `Description` | `nvarchar(2000)` | No |
| `RecordedAt` | `datetimeoffset` | No |
| `CreatedAt` | `datetimeoffset` | No |

- PrimaryKey: `PK_MedicalHistoryEntries`
- ForeignKey: `FK_MedicalHistoryEntries_Patients_PatientId`

## VitalsSamples

| Column | SQL type | Nullable |
|---|---|---|
| `Id` | `uniqueidentifier` | No |
| `PatientId` | `uniqueidentifier` | No |
| `DeviceId` | `uniqueidentifier` | No |
| `Timestamp` | `datetimeoffset` | No |
| `HeartRate` | `int` | No |
| `Spo2` | `int` | No |
| `CreatedAt` | `datetimeoffset` | No |

- PrimaryKey: `PK_VitalsSamples`
- ForeignKey: `FK_VitalsSamples_Devices_DeviceId`
- ForeignKey: `FK_VitalsSamples_Patients_PatientId`

## RiskAssessments

| Column | SQL type | Nullable |
|---|---|---|
| `Id` | `uniqueidentifier` | No |
| `PatientId` | `uniqueidentifier` | No |
| `EcgFeatureSetId` | `uniqueidentifier` | No |
| `Timestamp` | `datetimeoffset` | No |
| `StatusLevel` | `nvarchar(32)` | No |
| `ConfidenceScore` | `float` | No |
| `ModelVersion` | `nvarchar(40)` | No |
| `RiskScore` | `int` | Yes |
| `ContributingFactorsJson` | `nvarchar(2000)` | No |
| `CreatedAt` | `datetimeoffset` | No |

- PrimaryKey: `PK_RiskAssessments`
- ForeignKey: `FK_RiskAssessments_EcgFeatureSets_EcgFeatureSetId`
- ForeignKey: `FK_RiskAssessments_Patients_PatientId`

## Index definitions

See the migration for indexed column order and predicates.

- `Alerts` · `IX_Alerts_PatientId_Severity_ResolvedAt`
- `Alerts` · `IX_Alerts_PatientId_TriggeredAt`
- `AuditLogs` · `IX_AuditLogs_EntityType_EntityId_Timestamp`
- `AuditLogs` · `IX_AuditLogs_UserId`
- `Devices` · `IX_Devices_PatientId`
- `Devices` · `IX_Devices_SerialNumber`
- `DoctorPatientAssignments` · `IX_DoctorPatientAssignments_DoctorId_PatientId`
- `DoctorPatientAssignments` · `IX_DoctorPatientAssignments_PatientId`
- `DoctorPatientAssignments` · `UX_DoctorPatientAssignments_OnePrimaryPerPatient`
- `Doctors` · `IX_Doctors_LicenseNumber`
- `Doctors` · `IX_Doctors_UserId`
- `EcgFeatureSets` · `IX_EcgFeatureSets_PatientId_Timestamp`
- `ElectrodeVerifications` · `IX_ElectrodeVerifications_PatientId_Timestamp`
- `MedicalHistoryEntries` · `IX_MedicalHistoryEntries_PatientId`
- `Patients` · `IX_Patients_FileNumber`
- `Patients` · `IX_Patients_NationalId`
- `Patients` · `IX_Patients_UserId`
- `RefreshTokens` · `IX_RefreshTokens_TokenHash`
- `RefreshTokens` · `IX_RefreshTokens_UserId`
- `RiskAssessments` · `IX_RiskAssessments_EcgFeatureSetId`
- `RiskAssessments` · `IX_RiskAssessments_PatientId_Timestamp`
- `UserClaims` · `IX_UserClaims_UserId`
- `UserLogins` · `IX_UserLogins_UserId`
- `Users` · `EmailIndex`
- `Users` · `UserNameIndex`
- `VitalsSamples` · `IX_VitalsSamples_DeviceId`
- `VitalsSamples` · `IX_VitalsSamples_PatientId_Timestamp`
