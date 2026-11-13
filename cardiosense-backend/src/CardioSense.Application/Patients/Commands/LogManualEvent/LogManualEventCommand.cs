// =============================================================================
// File           : LogManualEventCommand.cs
// Layer          : Application / Patients/Commands/LogManualEvent
// Purpose        : Request for a doctor to log a manual clinical event on a patient.
// Logic          : Body of POST /api/patients/{id}/manual-events (note + severity). The event becomes an
//                  alert of type Manual Event, as the architecture doc states ('an alert is raised when a
//                  threshold is crossed or a manual event is logged').
// Implementation : Immutable record implementing the patient-scope marker so access is enforced by the
//                  pipeline.
// =============================================================================
using CardioSense.Application.Common.Dtos;
using CardioSense.Application.Common.Interfaces;
using CardioSense.Domain.Common;
using CardioSense.Domain.Enums;
using Mediator;

namespace CardioSense.Application.Patients.Commands.LogManualEvent;

/// <summary>Log a manual event.</summary>
public sealed record LogManualEventCommand(Guid PatientId, string Note, AlertSeverity Severity)
    : IRequest<Result<AlertDto>>, IPatientScopedRequest;
