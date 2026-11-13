// =============================================================================
// File           : StubAiService.cs
// Layer          : Tests / Support
// Purpose        : Replaces the Python AI service so integration tests need no network.
// Logic          : Only the AI is faked; everything else in the integration tests is real.
// Implementation : Always answers Stable and a fixed narrative.
// =============================================================================
using CardioSense.Application.Common.Interfaces;
using CardioSense.Application.Common.Models;
using CardioSense.Domain.Common;
using CardioSense.Domain.Enums;

namespace CardioSense.Api.IntegrationTests.Support;

/// <summary>An AI service that always says "Stable".</summary>
public sealed class StubAiService : IRiskAssessmentService
{
    /// <summary>The narrative every report request returns.</summary>
    private const string Narrative = "Stub narrative.";

    /// <inheritdoc />
    public Task<Result<RiskAssessmentResult>> AssessAsync(
        RiskAssessmentRequest request, CancellationToken ct = default) =>
        Task.FromResult(Result.Success(new RiskAssessmentResult(RiskStatusLevel.Stable, 0.9, "stub", 10, [])));

    /// <inheritdoc />
    public Task<Result<string>> GenerateReportNarrativeAsync(
        ReportNarrativeRequest request, CancellationToken ct = default) =>
        Task.FromResult(Result.Success(Narrative));
}
