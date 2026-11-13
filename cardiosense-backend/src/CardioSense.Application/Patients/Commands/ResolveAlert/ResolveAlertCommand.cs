// =============================================================================
// File           : ResolveAlertCommand.cs
// Layer          : Application / Patients/Commands/ResolveAlert
// Purpose        : Request for a doctor to resolve an alert (with an optional note).
// Logic          : Added so the documented domain rules are reachable: Alert.Resolve (critical alerts need a
//                  doctor) and the critical latch on RiskAssessment, which is released only when a doctor
//                  resolves the open critical alert.
// Implementation : Immutable record implementing the patient-scope marker; the endpoint itself arrives with
//                  the Api layer.
// =============================================================================
using CardioSense.Application.Common.Dtos;
using CardioSense.Application.Common.Interfaces;
using CardioSense.Domain.Common;
using Mediator;

namespace CardioSense.Application.Patients.Commands.ResolveAlert;

/// <summary>Resolve an alert.</summary>
public sealed record ResolveAlertCommand(Guid PatientId, Guid AlertId, string? ResolutionNote)
    : IRequest<Result<AlertDto>>, IPatientScopedRequest;
