// =============================================================================
// File           : GetPatientVitalsHistoryQuery.cs
// Layer          : Application / Patients/Queries/GetPatientVitalsHistory
// Purpose        : Query for a patient's down-sampled vitals history for a range (24h, 7d or 30d).
// Logic          : Serves GET /patients/{id}/vitals-history?range= and the /me variant; the range literal is
//                  validated.
// Implementation : Immutable record; Range stays a string because the contract literals ('24h') cannot be
//                  enum member names.
// =============================================================================
using CardioSense.Application.Common.Dtos;
using CardioSense.Application.Common.Interfaces;
using CardioSense.Domain.Common;
using Mediator;

namespace CardioSense.Application.Patients.Queries.GetPatientVitalsHistory;

/// <summary>Get a patient's vitals history.</summary>
public sealed record GetPatientVitalsHistoryQuery(Guid PatientId, string Range)
    : IRequest<Result<IReadOnlyList<VitalsSampleDto>>>, IPatientScopedRequest;
