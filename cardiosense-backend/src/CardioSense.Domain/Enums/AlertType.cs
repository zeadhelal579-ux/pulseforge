// =============================================================================
// File           : AlertType.cs
// Layer          : Domain / Enums
// Purpose        : What raised an alert.
// Logic          : The display text shown in the UI (for example Risk Threshold) is produced by the
//                  Application mapping.
// Implementation : Explicit numeric values so persisted data never shifts if members are reordered.
// =============================================================================
namespace CardioSense.Domain.Enums;

/// <summary>What raised an alert.</summary>
public enum AlertType
{
    /// <summary>Raised automatically when the AI assessment crossed a threshold.</summary>
    RiskThreshold = 0,

    /// <summary>Logged manually by a doctor.</summary>
    ManualEvent = 1
}
