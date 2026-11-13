// =============================================================================
// File           : ClinicalMappings.cs
// Layer          : Application / Common/Mappings
// Purpose        : Maps vitals, ECG features, risk assessments and electrode checks to their DTOs.
// Logic          : The risk assessment mapping takes includeClinicalDetails: patients get status only -
//                  confidence, score and flags are removed server-side (product rule: patients never see raw
//                  numbers).
// Implementation : Static extension methods; pure functions with no I/O.
// =============================================================================
using CardioSense.Application.Common.Dtos;
using CardioSense.Domain.Entities;
using CardioSense.Domain.Enums;

namespace CardioSense.Application.Common.Mappings;

/// <summary>Clinical data mappings.</summary>
public static class ClinicalMappings
{
    /// <summary>Maps a vitals sample.</summary>
    public static VitalsSampleDto ToDto(this VitalsSample sample) => new(
        sample.Id, sample.PatientId, sample.DeviceId, sample.Timestamp, sample.HeartRate, sample.Spo2);

    /// <summary>Maps an ECG feature set; QT and QTc are exposed as the pair [QT, QTc].</summary>
    public static EcgFeatureSetDto ToDto(this EcgFeatureSet features) => new(
        features.Id, features.PatientId, features.Timestamp, features.PrIntervalMs, features.QrsDurationMs,
        [features.QtMs, features.QtcMs]);

    /// <summary>Maps an assessment, hiding doctor-only fields unless includeClinicalDetails is true.</summary>
    public static RiskAssessmentDto ToDto(this RiskAssessment assessment, bool includeClinicalDetails) => new(
        assessment.Id,
        assessment.PatientId,
        assessment.Timestamp,
        assessment.StatusLevel,
        includeClinicalDetails ? assessment.ConfidenceScore : (double?)null,
        assessment.ModelVersion,
        includeClinicalDetails ? assessment.Flags : Array.Empty<string>(),
        includeClinicalDetails ? assessment.RiskScore : null);

    /// <summary>Maps an electrode verification.</summary>
    public static ElectrodeVerificationDto ToDto(this ElectrodeVerification verification) => new(
        verification.Id,
        verification.PatientId,
        verification.Timestamp,
        verification.PlacementStatus.ToDisplayText(),
        verification.ConfidenceScore);

    /// <summary>Readable label of a placement status (rendered verbatim by the UI).</summary>
    public static string ToDisplayText(this ElectrodePlacementStatus status) => status switch
    {
        ElectrodePlacementStatus.Optimal => "Optimal Placement",
        ElectrodePlacementStatus.RepositionNeeded => "Reposition Needed",
        _ => status.ToString()
    };
}
