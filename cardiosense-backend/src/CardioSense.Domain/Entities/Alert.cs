// =============================================================================
// File           : Alert.cs
// Layer          : Domain / Entities
// Purpose        : A clinical alert raised by the system (threshold crossed) or logged manually by a doctor.
// Logic          : Critical alerts can only be resolved by a doctor and an alert can be resolved only once -
//                  this is the rule that releases the RiskAssessment critical latch.
// Implementation : Sealed entity; Resolve returns a Result and receives the current time so the entity stays
//                  deterministic.
// =============================================================================
using CardioSense.Domain.Common;
using CardioSense.Domain.Enums;

namespace CardioSense.Domain.Entities;

/// <summary>A clinical alert.</summary>
public sealed class Alert : Entity
{
    /// <summary>Maximum length of the message and of the resolution note.</summary>
    public const int MessageMaxLength = 2000;

    /// <summary>Used by EF Core.</summary>
    private Alert()
    {
    }

    /// <summary>Builds an open alert after guarding every value.</summary>
    private Alert(Guid patientId, AlertType type, AlertSeverity severity, string message, DateTimeOffset now)
        : base(now)
    {
        PatientId = RequireId(patientId, nameof(patientId));
        Type = type;
        Severity = severity;
        Message = RequireText(message, nameof(message), MessageMaxLength);
        TriggeredAt = now;
    }

    /// <summary>The patient the alert is about.</summary>
    public Guid PatientId { get; private set; }

    /// <summary>What raised the alert.</summary>
    public AlertType Type { get; private set; }

    /// <summary>How serious the alert is.</summary>
    public AlertSeverity Severity { get; private set; }

    /// <summary>Human-readable message shown in the alerts timeline.</summary>
    public string Message { get; private set; } = null!;

    /// <summary>When the alert was raised.</summary>
    public DateTimeOffset TriggeredAt { get; private set; }

    /// <summary>When the alert was resolved; null while it is still open.</summary>
    public DateTimeOffset? ResolvedAt { get; private set; }

    /// <summary>Optional note explaining the resolution.</summary>
    public string? ResolutionNote { get; private set; }

    /// <summary>Creates a new open alert.</summary>
    public static Alert Create(Guid patientId, AlertType type, AlertSeverity severity, string message,
        DateTimeOffset now) =>
        new(patientId, type, severity, message, now);

    /// <summary>Resolves the alert; critical alerts require a doctor and nothing can be resolved twice.</summary>
    public Result Resolve(string? resolutionNote, bool resolvedByDoctor, DateTimeOffset now)
    {
        if (ResolvedAt is not null)
        {
            return Result.Failure(DomainErrors.Alert.AlreadyResolved);
        }

        if (Severity == AlertSeverity.Critical && !resolvedByDoctor)
        {
            return Result.Failure(DomainErrors.Alert.CriticalRequiresDoctor);
        }

        ResolvedAt = now;
        ResolutionNote = RequireOptionalText(resolutionNote, nameof(resolutionNote), MessageMaxLength);
        return Result.Success();
    }
}
