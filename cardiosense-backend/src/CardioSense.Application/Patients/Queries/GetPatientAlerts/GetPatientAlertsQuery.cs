// =============================================================================
// File           : GetPatientAlertsQuery.cs
// Layer          : Application / Patients/Queries/GetPatientAlerts
// Purpose        : Query for a patient's alert timeline (newest first).
// Logic          : Serves GET /patients/{id}/alerts and the /me variant.
// Implementation : Immutable record implementing the Mediator request contract and the patient-scope marker.
// =============================================================================
using CardioSense.Application.Common.Dtos;
using CardioSense.Application.Common.Interfaces;
using CardioSense.Domain.Common;
using Mediator;

namespace CardioSense.Application.Patients.Queries.GetPatientAlerts;

/// <summary>Get a patient's alerts.</summary>
public sealed record GetPatientAlertsQuery(Guid PatientId)
    : IRequest<Result<IReadOnlyList<AlertDto>>>, IPatientScopedRequest;
