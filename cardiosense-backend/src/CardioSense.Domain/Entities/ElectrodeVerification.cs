// =============================================================================
// File           : ElectrodeVerification.cs
// Layer          : Domain / Entities
// Purpose        : Result of the camera-based electrode placement check.
// Logic          : Placement quality is reported as a status plus a 0..1 confidence; the UI shows confidence
//                  as 'efficiency'.
// Implementation : Sealed entity; Create returns a Result because confidence comes from an external pipeline.
// =============================================================================
using CardioSense.Domain.Common;
using CardioSense.Domain.Enums;

namespace CardioSense.Domain.Entities;

/// <summary>An electrode placement verification.</summary>
public sealed class ElectrodeVerification : Entity
{
    /// <summary>Used by EF Core.</summary>
    private ElectrodeVerification()
    {
    }

    /// <summary>Builds a verification from already validated values.</summary>
    private ElectrodeVerification(
        Guid patientId, DateTimeOffset timestamp, ElectrodePlacementStatus placementStatus, double confidenceScore)
        : base(timestamp)
    {
        PatientId = RequireId(patientId, nameof(patientId));
        Timestamp = timestamp;
        PlacementStatus = placementStatus;
        ConfidenceScore = confidenceScore;
    }

    /// <summary>The patient that was checked.</summary>
    public Guid PatientId { get; private set; }

    /// <summary>When the check happened.</summary>
    public DateTimeOffset Timestamp { get; private set; }

    /// <summary>Outcome of the check.</summary>
    public ElectrodePlacementStatus PlacementStatus { get; private set; }

    /// <summary>Confidence of the check between 0 and 1.</summary>
    public double ConfidenceScore { get; private set; }

    /// <summary>Creates a verification, or fails when the confidence is outside 0..1.</summary>
    public static Result<ElectrodeVerification> Create(
        Guid patientId, DateTimeOffset timestamp, ElectrodePlacementStatus placementStatus, double confidenceScore)
    {
        if (double.IsNaN(confidenceScore) || confidenceScore is < 0 or > 1)
        {
            return Result.Failure<ElectrodeVerification>(DomainErrors.Electrode.ConfidenceOutOfRange);
        }

        return Result.Success(new ElectrodeVerification(patientId, timestamp, placementStatus, confidenceScore));
    }
}
