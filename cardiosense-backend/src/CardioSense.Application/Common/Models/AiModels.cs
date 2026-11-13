// =============================================================================
// File           : AiModels.cs
// Layer          : Application / Common/Models
// Purpose        : Request/response shapes of the AI service gateway (risk assessment and report narrative).
// Logic          : They mirror the documented POST /analyze contract (features + patient context in, status +
//                  confidence + flags out) plus an optional numeric risk score and the report narrative
//                  request.
// Implementation : Immutable records; the HTTP client in Infrastructure maps them to/from the Python
//                  service's JSON.
// =============================================================================
using CardioSense.Domain.Enums;

namespace CardioSense.Application.Common.Models;

/// <summary>Clinical context sent with every AI request.</summary>
public sealed record PatientContext(int Age, IReadOnlyList<string> PriorDiagnoses);

/// <summary>ECG features and vitals of one reading window.</summary>
public sealed record EcgFeatures(int PrIntervalMs, int QrsDurationMs, int QtMs, int QtcMs, int HeartRate, int Spo2);

/// <summary>Input of a risk assessment.</summary>
public sealed record RiskAssessmentRequest(Guid PatientId, EcgFeatures Features, PatientContext Context);

/// <summary>Output of a risk assessment (status, confidence, optional 0-100 score, doctor-facing flags).</summary>
public sealed record RiskAssessmentResult(
    RiskStatusLevel StatusLevel, double ConfidenceScore, string ModelVersion, int? RiskScore,
        IReadOnlyList<string> Flags);

/// <summary>Aggregated numbers of a report period.</summary>
public sealed record ReportStatistics(double? AverageHeartRate, int? HighestRiskScore, int AlertsLoggedCount);

/// <summary>Input of a report narrative (period literal, clinical context and statistics).</summary>
public sealed record ReportNarrativeRequest(string Period, PatientContext Context, ReportStatistics Statistics);
