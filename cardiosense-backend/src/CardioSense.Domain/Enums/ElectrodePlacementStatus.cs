// =============================================================================
// File           : ElectrodePlacementStatus.cs
// Layer          : Domain / Enums
// Purpose        : Result of the camera-based electrode placement check.
// Logic          : Optimal means readings can be trusted; RepositionNeeded means the electrodes should be
//                  moved.
// Implementation : Explicit numeric values so persisted data never shifts if members are reordered.
// =============================================================================
namespace CardioSense.Domain.Enums;

/// <summary>Result of the camera-based electrode placement check.</summary>
public enum ElectrodePlacementStatus
{
    /// <summary>Electrodes are placed correctly.</summary>
    Optimal = 0,

    /// <summary>Electrodes must be repositioned.</summary>
    RepositionNeeded = 1
}
