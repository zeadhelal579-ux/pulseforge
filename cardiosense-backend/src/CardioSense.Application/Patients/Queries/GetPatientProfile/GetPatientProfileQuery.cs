// =============================================================================
// File           : GetPatientProfileQuery.cs
// Layer          : Application / Patients/Queries/GetPatientProfile
// Purpose        : Query for one patient's full profile aggregate.
// Logic          : Serves GET /patients/me (the Api passes the caller's own patient id) and GET
//                  /patients/{id}; access is enforced by AuthorizationBehavior because the query is
//                  patient-scoped.
// Implementation : Immutable record implementing the Mediator request contract and the patient-scope marker.
// =============================================================================
using CardioSense.Application.Common.Dtos;
using CardioSense.Application.Common.Interfaces;
using CardioSense.Domain.Common;
using Mediator;

namespace CardioSense.Application.Patients.Queries.GetPatientProfile;

/// <summary>Get a patient's profile.</summary>
public sealed record GetPatientProfileQuery(Guid PatientId)
    : IRequest<Result<PatientProfileDto>>, IPatientScopedRequest;
