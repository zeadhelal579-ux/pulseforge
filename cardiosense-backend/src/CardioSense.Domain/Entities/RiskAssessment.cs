// =============================================================================
// File           : RiskAssessment.cs
// Layer          : Domain / Entities
// Purpose        : One AI evaluation of a patient's cardiac risk (Stable / Caution / Critical + confidence +
//                  flags).
// Logic          : Safety rule: while a critical alert is open the stored status can never drop below
//                  Critical - the system never auto-downgrades a critical patient; only a doctor resolving
//                  the alert releases it.
// Implementation : Sealed entity; the factory applies the latch and records it as the critical_latched flag.
//                  Flags are stored as JSON (column ContributingFactorsJson) by the persistence layer.
// =============================================================================
using CardioSense.Domain.Common;
using CardioSense.Domain.Enums;

namespace CardioSense.Domain.Entities;

/// <summary>An AI risk evaluation.</summary>
public sealed class RiskAssessment : Entity
{
    /// <summary>Flag added when the model said "better" but a critical alert is still open.</summary>
    public const string CriticalLatchedFlag = "critical_latched";

    /// <summary>Maximum length of the model version text.</summary>
    public const int ModelVersionMaxLength = 40;

    /// <summary>Used by EF Core.</summary>
    private RiskAssessment()
    {
    }

    /// <summary>Builds an assessment from already validated values.</summary>
    private RiskAssessment(
        Guid patientId, Guid ecgFeatureSetId, DateTimeOffset timestamp, RiskStatusLevel statusLevel,
        double confidenceScore, string modelVersion, int? riskScore, IReadOnlyList<string> flags)
        : base(timestamp)
    {
        PatientId = RequireId(patientId, nameof(patientId));
        EcgFeatureSetId = RequireId(ecgFeatureSetId, nameof(ecgFeatureSetId));
        Timestamp = timestamp;
        StatusLevel = statusLevel;
        ConfidenceScore = confidenceScore;
        ModelVersion = RequireText(modelVersion, nameof(modelVersion), ModelVersionMaxLength);
        RiskScore = riskScore;
        Flags = flags;
    }

    /// <summary>The patient that was assessed.</summary>
    public Guid PatientId { get; private set; }

    /// <summary>The feature set the assessment was computed from.</summary>
    public Guid EcgFeatureSetId { get; private set; }

    /// <summary>When the assessment was made.</summary>
    public DateTimeOffset Timestamp { get; private set; }

    /// <summary>Effective status (after the critical latch); what the patient UI reads.</summary>
    public RiskStatusLevel StatusLevel { get; private set; }

    /// <summary>Model confidence between 0 and 1 (doctor-only).</summary>
    public double ConfidenceScore { get; private set; }

    /// <summary>Version of the model that produced the assessment.</summary>
    public string ModelVersion { get; private set; } = null!;

    /// <summary>Optional numeric risk score 0-100 (doctor-only).</summary>
    public int? RiskScore { get; private set; }

    /// <summary>Doctor-facing flags such as possible_st_elevation.</summary>
    public IReadOnlyList<string> Flags { get; private set; } = [];

    /// <summary>
    /// Creates an assessment. When <paramref name="criticalLatchActive"/> is true and the model reports a
    /// lower level, the stored level stays Critical and the latch flag is added.
    /// </summary>
    public static Result<RiskAssessment> Create(
        Guid patientId, Guid ecgFeatureSetId, DateTimeOffset timestamp, RiskStatusLevel modelStatusLevel,
        double confidenceScore, string modelVersion, int? riskScore, IEnumerable<string> flags,
        bool criticalLatchActive)
    {
        if (double.IsNaN(confidenceScore) || confidenceScore is < 0 or > 1)
        {
            return Result.Failure<RiskAssessment>(DomainErrors.Risk.ConfidenceOutOfRange);
        }

        if (riskScore is < 0 or > 100)
        {
            return Result.Failure<RiskAssessment>(DomainErrors.Risk.RiskScoreOutOfRange);
        }

        var effectiveLevel = criticalLatchActive && modelStatusLevel < RiskStatusLevel.Critical
            ? RiskStatusLevel.Critical
            : modelStatusLevel;

        var cleanFlags = flags
            .Where(flag => !string.IsNullOrWhiteSpace(flag))
            .Select(flag => flag.Trim())
            .Distinct()
            .ToList();

        if (effectiveLevel != modelStatusLevel)
        {
            cleanFlags.Add(CriticalLatchedFlag);
        }

        return Result.Success(new RiskAssessment(
            patientId, ecgFeatureSetId, timestamp, effectiveLevel, confidenceScore, modelVersion, riskScore,
            cleanFlags));
    }
}
