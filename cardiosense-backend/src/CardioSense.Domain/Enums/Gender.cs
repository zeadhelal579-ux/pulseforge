// =============================================================================
// File           : Gender.cs
// Layer          : Domain / Enums
// Purpose        : Patient gender as collected at signup.
// Logic          : Mapped to the lower-case strings male/female used by the frontend contract.
// Implementation : Explicit numeric values so persisted data never shifts if members are reordered.
// =============================================================================
namespace CardioSense.Domain.Enums;

/// <summary>Patient gender as collected at signup.</summary>
public enum Gender
{
    /// <summary>Male.</summary>
    Male = 0,

    /// <summary>Female.</summary>
    Female = 1
}
