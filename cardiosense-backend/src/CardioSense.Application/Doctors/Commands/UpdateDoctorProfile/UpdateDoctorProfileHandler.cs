// =============================================================================
// File           : UpdateDoctorProfileHandler.cs
// Layer          : Application / Doctors/Commands/UpdateDoctorProfile
// Purpose        : Handles a doctor's own profile update and returns the refreshed profile.
// Logic          : Professional fields are saved first, then the language preference on the account, then the
//                  profile is re-read so the response reflects both.
// Implementation : Internal sealed handler; the frontend sends only the language today, but every editable
//                  field is supported.
// =============================================================================
using CardioSense.Application.Common.Dtos;
using CardioSense.Application.Common.Errors;
using CardioSense.Application.Common.Interfaces;
using CardioSense.Application.Common.Mappings;
using CardioSense.Domain.Common;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace CardioSense.Application.Doctors.Commands.UpdateDoctorProfile;

/// <summary>Handles <see cref="UpdateDoctorProfileCommand"/>.</summary>
internal sealed class UpdateDoctorProfileHandler(IAppDbContext db, IIdentityService identity, ICurrentUser currentUser)
    : IRequestHandler<UpdateDoctorProfileCommand, Result<DoctorProfileDto>>
{
    /// <summary>Applies the changes and returns the updated profile.</summary>
    public async ValueTask<Result<DoctorProfileDto>> Handle(UpdateDoctorProfileCommand request, CancellationToken ct)
    {
        if (currentUser.DoctorId is not { } doctorId)
        {
            return Result.Failure<DoctorProfileDto>(ApplicationErrors.Doctor.DoctorOnly);
        }

        var doctor = await db.Doctors.FirstOrDefaultAsync(d => d.Id == doctorId, ct);
        if (doctor is null)
        {
            return Result.Failure<DoctorProfileDto>(ApplicationErrors.Doctor.NotFound);
        }

        doctor.UpdateProfile(request.FullName, request.Phone, request.Specialty, request.Hospital, request.Department);
        await db.SaveChangesAsync(ct);

        if (request.PreferredLanguage is not null)
        {
            var languageUpdate = await identity.UpdatePreferredLanguageAsync(doctor.UserId, request.PreferredLanguage,
                ct);
            if (languageUpdate.IsFailure)
            {
                return Result.Failure<DoctorProfileDto>(languageUpdate.Error);
            }
        }

        var account = await identity.FindByIdAsync(doctor.UserId, ct);
        return account is null
            ? Result.Failure<DoctorProfileDto>(ApplicationErrors.Doctor.NotFound)
            : Result.Success(doctor.ToProfileDto(account));
    }
}
