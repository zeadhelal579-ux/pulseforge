// =============================================================================
// File           : IRiskAssessmentService.cs
// Layer          : Application / Common/Interfaces
// Purpose        : Gateway to the AI service: risk assessment of a reading window and the report narrative
//                  text.
// Logic          : Failures (service down, timeout, invalid answer) come back as Error.Unavailable so a
//                  broken AI service never breaks ingestion or the dashboards.
// Implementation : Implemented by HttpRiskAssessmentService in Infrastructure over internal HTTP/JSON to the
//                  Python FastAPI service.
// =============================================================================
using CardioSense.Application.Common.Models;
using CardioSense.Domain.Common;

namespace CardioSense.Application.Common.Interfaces;

/// <summary>Talks to the AI/ML service.</summary>
public interface IRiskAssessmentService
{
    /// <summary>Assesses one reading window (POST /analyze).</summary>
    Task<Result<RiskAssessmentResult>> AssessAsync(RiskAssessmentRequest request, CancellationToken ct = default);

    /// <summary>Generates the natural-language report summary for a period (POST /report).</summary>
    Task<Result<string>> GenerateReportNarrativeAsync(ReportNarrativeRequest request, CancellationToken ct = default);
}
