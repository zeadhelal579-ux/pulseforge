// =============================================================================
// File           : AlertDto.cs
// Layer          : Application / Common/Dtos
// Purpose        : A clinical alert as shown in the alerts timeline (also the payload of the
//                  alert:{patientId} push).
// Logic          : Type is the human-readable text the UI renders verbatim (for example 'Manual Event').
// Implementation : Immutable positional records; property names are the PascalCase twins of the TypeScript
//                  fields (camelCase on the wire).
// =============================================================================
using CardioSense.Domain.Enums;

namespace CardioSense.Application.Common.Dtos;

/// <summary>A clinical alert.</summary>
public sealed record AlertDto(
    Guid Id,
    Guid PatientId,
    string Type,
    AlertSeverity Severity,
    string Message,
    DateTimeOffset TriggeredAt,
    DateTimeOffset? ResolvedAt,
    string? ResolutionNote);
