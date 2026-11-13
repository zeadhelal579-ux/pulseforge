// =============================================================================
// File           : RiskAssessmentTests.cs
// Layer          : Tests / Entities
// Purpose        : Verifies the most important safety rule: a Critical status never drops automatically.
// Logic          : While a critical alert is open the stored status stays Critical even if the AI later says
//                  "better"; only a doctor resolving the alert lifts it. The entity enforces this through the
//                  "critical latch" so no use case can forget it.
// Implementation : Plain xUnit facts and theories; no database.
// =============================================================================
using CardioSense.Application.Tests.Support;
using CardioSense.Domain.Common;
using CardioSense.Domain.Entities;
using CardioSense.Domain.Enums;

namespace CardioSense.Application.Tests.Entities;

/// <summary>Tests for the RiskAssessment entity.</summary>
public sealed class RiskAssessmentTests
{
    /// <summary>The instant used as "now".</summary>
    private static readonly DateTimeOffset Now = ApplicationTestHost.Start;

    /// <summary>Creates an assessment with the given model output and latch state.</summary>
    private static Result<RiskAssessment> Create(
        RiskStatusLevel modelLevel, bool latchActive, double confidence = 0.9, int? riskScore = 20,
        params string[] flags) =>
        RiskAssessment.Create(
            Guid.NewGuid(), Guid.NewGuid(), Now, modelLevel, confidence, "test-model", riskScore, flags, latchActive);

    /// <summary>With the latch active an improved model status is stored as Critical, and flagged.</summary>
    [Fact]
    public void An_active_critical_latch_keeps_the_status_critical()
    {
        var result = Create(RiskStatusLevel.Stable, latchActive: true);

        Assert.True(result.IsSuccess);
        Assert.Equal(RiskStatusLevel.Critical, result.Value.StatusLevel);
        Assert.Contains("critical_latched", result.Value.Flags);
    }

    /// <summary>Without the latch the model's status is stored as it is.</summary>
    [Fact]
    public void Without_the_latch_the_model_status_is_kept()
    {
        var result = Create(RiskStatusLevel.Caution, latchActive: false);

        Assert.True(result.IsSuccess);
        Assert.Equal(RiskStatusLevel.Caution, result.Value.StatusLevel);
        Assert.DoesNotContain("critical_latched", result.Value.Flags);
    }

    /// <summary>A model that is already Critical is not flagged as latched.</summary>
    [Fact]
    public void A_critical_model_status_is_not_flagged_as_latched()
    {
        var result = Create(RiskStatusLevel.Critical, latchActive: true);

        Assert.Equal(RiskStatusLevel.Critical, result.Value.StatusLevel);
        Assert.DoesNotContain("critical_latched", result.Value.Flags);
    }

    /// <summary>Blank and duplicate flags from the AI are cleaned up.</summary>
    [Fact]
    public void Flags_are_trimmed_and_deduplicated()
    {
        var result = Create(
            RiskStatusLevel.Caution, false, 0.9, 20, " prolonged_qtc ", "prolonged_qtc", " ", "wide_qrs");

        Assert.Equal(new[] { "prolonged_qtc", "wide_qrs" }, result.Value.Flags);
    }

    /// <summary>A confidence outside 0..1 is rejected, not stored.</summary>
    [Theory]
    [InlineData(-0.01)]
    [InlineData(1.01)]
    [InlineData(double.NaN)]
    public void Confidence_outside_zero_to_one_is_rejected(double confidence)
    {
        var result = Create(RiskStatusLevel.Stable, false, confidence);

        Assert.True(result.IsFailure);
        Assert.Equal(DomainErrors.Risk.ConfidenceOutOfRange.Code, result.Error.Code);
    }

    /// <summary>A risk score outside 0..100 is rejected, not stored.</summary>
    [Theory]
    [InlineData(-1)]
    [InlineData(101)]
    public void A_risk_score_outside_zero_to_one_hundred_is_rejected(int riskScore)
    {
        var result = Create(RiskStatusLevel.Stable, false, 0.9, riskScore);

        Assert.True(result.IsFailure);
        Assert.Equal(DomainErrors.Risk.RiskScoreOutOfRange.Code, result.Error.Code);
    }
}
