// =============================================================================
// File           : AiContracts.cs
// Layer          : Infrastructure / AiClient
// Purpose        : The JSON bodies exchanged with the Python AI service (POST /analyze and POST /report).
// Logic          : These records ARE the wire contract the cardiosense-ai service must implement. Property names
//                  serialise as camelCase (priorDiagnosis, qtQtcMs = [QT, QTc]). Everything the answer must contain is
//                  nullable here so a missing field is detected instead of silently becoming 0 / Stable - a missing
//                  statusLevel must never read as "all clear".
// Implementation : Internal sealed records plus one shared JsonSerializerOptions (web defaults, enum names only).
// =============================================================================
using System.Text.Json;
using System.Text.Json.Serialization;
using CardioSense.Domain.Enums;

namespace CardioSense.Infrastructure.AiClient;

/// <summary>Clinical context of a patient (age and prior diagnoses).</summary>
internal sealed record AiPatientContext(int Age, IReadOnlyList<string> PriorDiagnosis);

/// <summary>Features of one reading window; QtQtcMs is [QT, QTc].</summary>
internal sealed record AiFeatures(int PrIntervalMs, int QrsDurationMs, int[] QtQtcMs, int HeartRate, int Spo2);

/// <summary>Body of POST /analyze.</summary>
internal sealed record AnalyzeRequestBody(Guid PatientId, AiFeatures Features, AiPatientContext PatientContext);

/// <summary>Answer of POST /analyze; every field except the optional ones must be present.</summary>
internal sealed record AnalyzeResponseBody(
    RiskStatusLevel? StatusLevel,
    double? ConfidenceScore,
    string? ModelVersion,
    int? RiskScore,
    IReadOnlyList<string>? Flags);

/// <summary>Aggregated numbers of the report period.</summary>
internal sealed record AiReportStatistics(double? AverageHeartRate, int? HighestRiskScore, int AlertsLoggedCount);

/// <summary>Body of POST /report.</summary>
internal sealed record ReportRequestBody(string Period, AiPatientContext PatientContext, AiReportStatistics Statistics);

/// <summary>Answer of POST /report.</summary>
internal sealed record ReportResponseBody(string? NarrativeSummary);

/// <summary>JSON settings shared by every AI request and answer.</summary>
internal static class AiJson
{
    /// <summary>Web defaults (camelCase, case-insensitive) with enums as names and no numeric enum values.</summary>
    public static readonly JsonSerializerOptions Options = new(JsonSerializerDefaults.Web)
    {
        Converters = { new JsonStringEnumConverter(allowIntegerValues: false) }
    };
}
