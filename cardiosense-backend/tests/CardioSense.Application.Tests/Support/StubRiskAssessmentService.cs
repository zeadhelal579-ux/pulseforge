// =============================================================================
// File           : StubRiskAssessmentService.cs
// Layer          : Tests / Support
// Purpose        : IRiskAssessmentService whose answer the test chooses.
// Logic          : The Python AI service is not part of Application tests. A test sets NextResult to a status (or to
//                  "unavailable") and checks how the handler reacts.
// Implementation : Returns the configured result and remembers every request it received.
// =============================================================================
using CardioSense.Application.Common.Interfaces;
using CardioSense.Application.Common.Models;
using CardioSense.Domain.Common;
using CardioSense.Domain.Enums;

namespace CardioSense.Application.Tests.Support;

/// <summary>A scripted AI service.</summary>
public sealed class StubRiskAssessmentService : IRiskAssessmentService
{
    /// <summary>What the next AssessAsync call returns.</summary>
    public Result<RiskAssessmentResult> NextResult { get; set; } = Answer(RiskStatusLevel.Stable);

    /// <summary>The narrative every report request returns.</summary>
    private const string Narrative = "Test narrative.";

    /// <summary>Every assessment request received.</summary>
    public List<RiskAssessmentRequest> Requests { get; } = [];

    /// <summary>Builds a successful AI answer with the given status and optional flags.</summary>
    public static Result<RiskAssessmentResult> Answer(RiskStatusLevel level, params string[] flags) =>
        Result.Success(new RiskAssessmentResult(level, 0.9, "test-model", 20, flags));

    /// <inheritdoc />
    public Task<Result<RiskAssessmentResult>> AssessAsync(RiskAssessmentRequest request, CancellationToken ct = default)
    {
        Requests.Add(request);
        return Task.FromResult(NextResult);
    }

    /// <inheritdoc />
    public Task<Result<string>> GenerateReportNarrativeAsync(
        ReportNarrativeRequest request, CancellationToken ct = default) =>
        Task.FromResult(Result.Success(Narrative));
}
