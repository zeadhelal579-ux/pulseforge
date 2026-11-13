// =============================================================================
// File           : ClinicalDtos.cs
// Layer          : Application / Common/Dtos
// Purpose        : Latest-reading DTOs: vitals sample, ECG feature set, AI risk assessment, electrode
//                  verification.
// Logic          : Doctor-only fields of the risk assessment (confidence, score, flags) are nullable/empty so
//                  they can be redacted for patient callers without changing the shape.
// Implementation : Immutable positional records; property names are the PascalCase twins of the TypeScript
//                  fields (camelCase on the wire). Grouped in one file because they always travel together
//                  inside the patient aggregate.
// =============================================================================
using CardioSense.Domain.Enums;

namespace CardioSense.Application.Common.Dtos;

/// <summary>One heart-rate / SpO2 reading (Spo2 serialises as 'spo2').</summary>
public sealed record VitalsSampleDto(
    Guid Id, Guid PatientId, Guid DeviceId, DateTimeOffset Timestamp, int HeartRate, int Spo2);

/// <summary>Extracted ECG intervals; QtQtcMs is [QT, QTc].</summary>
public sealed record EcgFeatureSetDto(
    Guid Id, Guid PatientId, DateTimeOffset Timestamp, int PrIntervalMs, int QrsDurationMs, int[] QtQtcMs);

/// <summary>An AI assessment; confidence, score and flags are null/empty for patient callers.</summary>
public sealed record RiskAssessmentDto(
    Guid Id,
    Guid PatientId,
    DateTimeOffset Timestamp,
    RiskStatusLevel StatusLevel,
    double? ConfidenceScore,
    string ModelVersion,
    IReadOnlyList<string> Flags,
    int? RiskScore);

/// <summary>An electrode placement check; ConfidenceScore is 0..1.</summary>
public sealed record ElectrodeVerificationDto(
    Guid Id, Guid PatientId, DateTimeOffset Timestamp, string PlacementStatus, double ConfidenceScore);
