// =============================================================================
// File           : RealtimePayloads.cs
// Layer          : Application / Common/Dtos
// Purpose        : Payloads pushed over SignalR: vitals tick, risk status (clinical + patient-safe variants),
//                  doctor list summary.
// Logic          : RiskStatusBroadcast carries both variants so the Application layer - which owns the
//                  'patients never see raw numbers' rule - decides what each audience gets; the notifier only
//                  routes them.
// Implementation : Immutable positional records; property names are the PascalCase twins of the TypeScript
//                  fields (camelCase on the wire).
// =============================================================================
using CardioSense.Domain.Enums;

namespace CardioSense.Application.Common.Dtos;

/// <summary>Heart-rate / SpO2 tick (event vitals:{patientId}).</summary>
public sealed record VitalsTickPayload(Guid PatientId, int HeartRate, int Spo2, DateTimeOffset Timestamp);

/// <summary>Risk status change (event riskStatus:{patientId}).</summary>
public sealed record RiskStatusPayload(
    Guid PatientId,
    RiskStatusLevel StatusLevel,
    double? ConfidenceScore,
    int? RiskScore,
    IReadOnlyList<string> Flags,
    DateTimeOffset Timestamp);

/// <summary>The two audience-specific variants of one risk status change.</summary>
public sealed record RiskStatusBroadcast(RiskStatusPayload Clinical, RiskStatusPayload PatientView);

/// <summary>Light summary for the doctor's list (event patientSummary:{doctorId}).</summary>
public sealed record PatientSummaryUpdate(
    Guid PatientId, RiskStatusLevel StatusLevel, string StatusLabel, int HeartRate, int Spo2, bool IsLive);
