// =============================================================================
// File           : AlertMappings.cs
// Layer          : Application / Common/Mappings
// Purpose        : Maps an Alert entity to AlertDto and turns AlertType into the text the UI shows.
// Logic          : The frontend renders Alert.type verbatim, so the enum is converted to a readable label
//                  here.
// Implementation : Static extension methods; pure functions with no I/O.
// =============================================================================
using CardioSense.Application.Common.Dtos;
using CardioSense.Domain.Entities;
using CardioSense.Domain.Enums;

namespace CardioSense.Application.Common.Mappings;

/// <summary>Alert mappings.</summary>
public static class AlertMappings
{
    /// <summary>Maps an alert to its DTO.</summary>
    public static AlertDto ToDto(this Alert alert) => new(
        alert.Id,
        alert.PatientId,
        alert.Type.ToDisplayText(),
        alert.Severity,
        alert.Message,
        alert.TriggeredAt,
        alert.ResolvedAt,
        alert.ResolutionNote);

    /// <summary>Readable label of an alert type.</summary>
    public static string ToDisplayText(this AlertType type) => type switch
    {
        AlertType.RiskThreshold => "Risk Threshold",
        AlertType.ManualEvent => "Manual Event",
        _ => type.ToString()
    };
}
