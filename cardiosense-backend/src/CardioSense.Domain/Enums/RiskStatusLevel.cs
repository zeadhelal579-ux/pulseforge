// =============================================================================
// File           : RiskStatusLevel.cs
// Layer          : Domain / Enums
// Purpose        : Patient status shown in the UI (Stable, Caution, Critical).
// Logic          : Ordered by severity (higher value = worse) so Max() over a set of levels yields the worst
//                  one.
// Implementation : Explicit numeric values so persisted data never shifts if members are reordered.
// =============================================================================
namespace CardioSense.Domain.Enums;

/// <summary>Patient status shown in the UI (Stable, Caution, Critical).</summary>
public enum RiskStatusLevel
{
    /// <summary>No concern detected.</summary>
    Stable = 0,

    /// <summary>Needs attention.</summary>
    Caution = 1,

    /// <summary>Needs immediate attention.</summary>
    Critical = 2
}
