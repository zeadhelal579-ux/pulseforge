// =============================================================================
// File           : GetMyPatientsHandler.cs
// Layer          : Application / Doctors/Queries/GetMyPatients
// Purpose        : Handles the doctor's patient list.
// Logic          : Takes only the doctor's own caseload, filters by status and search text, orders the
//                  sickest first (Critical, Caution, Stable, then name) and pages the result. A caseload is
//                  small (dozens), so filtering/paging happens in memory after one batch of queries - simple
//                  and independent of SQL translation quirks.
// Implementation : Internal sealed handler; rows come from PatientListItemAssembler, search matches the name
//                  or the file number.
// =============================================================================
using CardioSense.Application.Common.Contracts;
using CardioSense.Application.Common.Dtos;
using CardioSense.Application.Common.Errors;
using CardioSense.Application.Common.Interfaces;
using CardioSense.Application.Doctors.Common;
using CardioSense.Domain.Common;
using Mediator;

namespace CardioSense.Application.Doctors.Queries.GetMyPatients;

/// <summary>Handles <see cref="GetMyPatientsQuery"/>.</summary>
internal sealed class GetMyPatientsHandler(
    IAppDbContext db, ICurrentUser currentUser, PatientListItemAssembler assembler)
    : IRequestHandler<GetMyPatientsQuery, Result<PagedResult<PatientListItemDto>>>
{
    /// <summary>Builds one page of the doctor's patient list.</summary>
    public async ValueTask<Result<PagedResult<PatientListItemDto>>> Handle(
        GetMyPatientsQuery request, CancellationToken ct)
    {
        if (currentUser.DoctorId is not { } doctorId)
        {
            return Result.Failure<PagedResult<PatientListItemDto>>(ApplicationErrors.Doctor.DoctorOnly);
        }

        var patientIds = await db.GetPatientIdsAsync(doctorId, ct);
        var rows = await assembler.BuildAsync(patientIds, ct);

        IEnumerable<PatientListItemDto> matches = rows;

        if (ContractLiterals.PatientListFilters.ToLevel(request.Status) is { } level)
        {
            matches = matches.Where(row => row.StatusLevel == level);
        }

        var term = request.Search?.Trim();
        if (!string.IsNullOrEmpty(term))
        {
            matches = matches.Where(row =>
                row.FullName.Contains(term, StringComparison.OrdinalIgnoreCase)
                || row.FileNumber.Contains(term, StringComparison.OrdinalIgnoreCase));
        }

        var ordered = matches
            .OrderByDescending(row => row.StatusLevel)
            .ThenBy(row => row.FullName)
            .ToList();

        var page = ordered
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToList();
        var totalPages = (int)Math.Ceiling(ordered.Count / (double)request.PageSize);

        return Result.Success(new PagedResult<PatientListItemDto>(
            page, request.Page, request.PageSize, ordered.Count, totalPages));
    }
}
