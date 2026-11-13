// =============================================================================
// File           : AlertSeverity.cs
// Layer          : Domain / Enums
// Purpose        : Severity of an alert (matches the badges shown in the dashboards).
// Logic          : Ordered by severity; only Critical alerts have the doctor-only resolution rule.
// Implementation : Explicit numeric values so persisted data never shifts if members are reordered.
// =============================================================================
namespace CardioSense.Domain.Enums;

/// <summary>Severity of an alert (matches the badges shown in the dashboards).</summary>
public enum AlertSeverity
{
    /// <summary>Informational note.</summary>
    Info = 0,

    /// <summary>Short-lived blip that resolves by itself.</summary>
    Transient = 1,

    /// <summary>Needs attention soon.</summary>
    Moderate = 2,

    /// <summary>Needs immediate attention.</summary>
    Critical = 3
}
