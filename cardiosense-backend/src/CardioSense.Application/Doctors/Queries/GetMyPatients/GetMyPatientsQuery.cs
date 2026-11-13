// =============================================================================
// File           : GetMyPatientsQuery.cs
// Layer          : Application / Doctors/Queries/GetMyPatients
// Purpose        : Query for the doctor's patient list (status filter, search, paging).
// Logic          : Serves GET /api/doctors/me/patients?status=&search=&page=. Defaults match what the
//                  frontend sends when a parameter is omitted; the default page size (5) must match
//                  PATIENT_LIST_PAGE_SIZE in the frontend.
// Implementation : Immutable record with default parameter values; the filter literals are 'all', 'stable',
//                  'alert', 'critical'.
// =============================================================================
using CardioSense.Application.Common.Contracts;
using CardioSense.Application.Common.Dtos;
using CardioSense.Domain.Common;
using Mediator;

namespace CardioSense.Application.Doctors.Queries.GetMyPatients;

/// <summary>Get the doctor's patients.</summary>
public sealed record GetMyPatientsQuery(
    string Status = ContractLiterals.PatientListFilters.All,
    string? Search = null,
    int Page = 1,
    int PageSize = GetMyPatientsQuery.DefaultPageSize) : IRequest<Result<PagedResult<PatientListItemDto>>>
{
    /// <summary>Rows per page when the client sends no page size.</summary>
    public const int DefaultPageSize = 5;

    /// <summary>Largest page size a client may request.</summary>
    public const int MaxPageSize = 50;
}
