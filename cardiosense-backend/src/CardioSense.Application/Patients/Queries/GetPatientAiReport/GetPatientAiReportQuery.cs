// =============================================================================
// File           : GetPatientAiReportQuery.cs
// Layer          : Application / Patients/Queries/GetPatientAiReport
// Purpose        : Query for the AI clinical report of a patient for a period (daily, weekly or monthly).
// Logic          : Serves GET /patients/{id}/ai-report?period= and the /me variant (an endpoint the
//                  architecture doc did not list but the frontend calls).
// Implementation : Immutable record; Period stays a lower-case string literal as in the contract.
// =============================================================================
using CardioSense.Application.Common.Dtos;
using CardioSense.Application.Common.Interfaces;
using CardioSense.Domain.Common;
using Mediator;

namespace CardioSense.Application.Patients.Queries.GetPatientAiReport;

/// <summary>Get a patient's AI report.</summary>
public sealed record GetPatientAiReportQuery(Guid PatientId, string Period)
    : IRequest<Result<AiReportDto>>, IPatientScopedRequest;
