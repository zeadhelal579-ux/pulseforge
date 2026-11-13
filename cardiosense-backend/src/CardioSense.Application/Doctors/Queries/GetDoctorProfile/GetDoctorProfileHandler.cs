// =============================================================================
// File           : GetDoctorProfileHandler.cs
// Layer          : Application / Doctors/Queries/GetDoctorProfile
// Purpose        : Handles GetDoctorProfileQuery.
// Logic          : The doctor is whoever is authenticated; the account's e-mail and language come from the
//                  identity service.
// Implementation : Internal sealed handler with one profile query and one account lookup.
// =============================================================================
using CardioSense.Application.Common.Dtos;
using CardioSense.Application.Common.Errors;
using CardioSense.Application.Common.Interfaces;
using CardioSense.Application.Common.Mappings;
using CardioSense.Domain.Common;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace CardioSense.Application.Doctors.Queries.GetDoctorProfile;

/// <summary>Handles <see cref="GetDoctorProfileQuery"/>.</summary>
internal sealed class GetDoctorProfileHandler(IAppDbContext db, IIdentityService identity, ICurrentUser currentUser)
    : IRequestHandler<GetDoctorProfileQuery, Result<DoctorProfileDto>>
{
    /// <summary>Loads the doctor's profile.</summary>
    public async ValueTask<Result<DoctorProfileDto>> Handle(GetDoctorProfileQuery request, CancellationToken ct)
    {
        if (currentUser.DoctorId is not { } doctorId)
        {
            return Result.Failure<DoctorProfileDto>(ApplicationErrors.Doctor.DoctorOnly);
        }

        var doctor = await db.Doctors.AsNoTracking().FirstOrDefaultAsync(d => d.Id == doctorId, ct);
        var account = doctor is null ? null : await identity.FindByIdAsync(doctor.UserId, ct);

        return doctor is null || account is null
            ? Result.Failure<DoctorProfileDto>(ApplicationErrors.Doctor.NotFound)
            : Result.Success(doctor.ToProfileDto(account));
    }
}
