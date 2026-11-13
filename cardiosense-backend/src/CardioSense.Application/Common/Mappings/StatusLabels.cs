// =============================================================================
// File           : StatusLabels.cs
// Layer          : Application / Common/Mappings
// Purpose        : The label shown on a status pill (Fully Stable / Clinical Alert / Critical Alert /
//                  Awaiting Data).
// Logic          : A patient with no assessment yet is labelled 'Awaiting Data' instead of silently looking
//                  healthy.
// Implementation : Static helper with one switch expression.
// =============================================================================
using CardioSense.Domain.Enums;

namespace CardioSense.Application.Common.Mappings;

/// <summary>Status pill labels.</summary>
public static class StatusLabels
{
    /// <summary>Label of a patient that has no assessment yet.</summary>
    public const string AwaitingData = "Awaiting Data";

    /// <summary>Label of the "Alert" kind used by the doctor's list for Caution and Critical.</summary>
    public const string ClinicalAlertKind = "Clinical";

    /// <summary>Returns the label for a status level.</summary>
    public static string For(RiskStatusLevel level, bool hasAssessment) =>
        !hasAssessment
            ? AwaitingData
            : level switch
            {
                RiskStatusLevel.Critical => "Critical Alert",
                RiskStatusLevel.Caution => "Clinical Alert",
                _ => "Fully Stable"
            };
}
