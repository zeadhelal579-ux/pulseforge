// =============================================================================
// File           : AiReportDto.cs
// Layer          : Application / Common/Dtos
// Purpose        : AI clinical report for a period: narrative text plus the numbers behind it.
// Logic          : The narrative comes from the AI service and is displayed verbatim; the numbers are
//                  computed from stored data.
// Implementation : Immutable positional records; property names are the PascalCase twins of the TypeScript
//                  fields (camelCase on the wire). Period is the lower-case literal (daily/weekly/monthly).
// =============================================================================
namespace CardioSense.Application.Common.Dtos;

/// <summary>An AI report.</summary>
public sealed record AiReportDto(
    string Period,
    string NarrativeSummary,
    double AverageHeartRate,
    int? HighestRiskReading,
    int AlertsLoggedCount,
    DateTimeOffset GeneratedAt);
