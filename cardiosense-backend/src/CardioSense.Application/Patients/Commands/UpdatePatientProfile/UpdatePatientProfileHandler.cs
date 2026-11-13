// =============================================================================
// File           : UpdatePatientProfileHandler.cs
// Layer          : Application / Patients/Commands/UpdatePatientProfile
// Purpose        : Handles a patient's own profile update and returns the refreshed aggregate.
// Logic          : Only the patient may edit their profile (an assigned doctor passes the access check but
//                  not this one). Contact fields are saved first, then the language preference on the
//                  account.
// Implementation : Internal sealed handler; the response reuses PatientProfileAssembler so it has the same
//                  shape as GET /patients/me.
// =============================================================================
using CardioSense.Application.Common.Dtos;
using CardioSense.Application.Common.Errors;
using CardioSense.Application.Common.Interfaces;
using CardioSense.Application.Patients.Common;
using CardioSense.Domain.Common;
using CardioSense.Domain.Enums;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace CardioSense.Application.Patients.Commands.UpdatePatientProfile;

/// <summary>Handles <see cref="UpdatePatientProfileCommand"/>.</summary>
internal sealed class UpdatePatientProfileHandler(
    IAppDbContext db, IIdentityService identity, ICurrentUser currentUser, PatientProfileAssembler assembler)
    : IRequestHandler<UpdatePatientProfileCommand, Result<PatientProfileDto>>
{
    /// <summary>Applies the changes and returns the updated profile.</summary>
    public async ValueTask<Result<PatientProfileDto>> Handle(UpdatePatientProfileCommand request, CancellationToken ct)
    {
        if (currentUser.Role != UserRole.Patient || currentUser.PatientId != request.PatientId)
        {
            return Result.Failure<PatientProfileDto>(ApplicationErrors.Patient.OnlySelfCanEdit);
        }

        var patient = await db.Patients.FirstOrDefaultAsync(p => p.Id == request.PatientId, ct);
        if (patient is null)
        {
            return Result.Failure<PatientProfileDto>(ApplicationErrors.Patient.NotFound);
        }

        patient.UpdateContact(request.FullName, request.Phone);
        await db.SaveChangesAsync(ct);

        if (request.PreferredLanguage is not null)
        {
            var languageUpdate = await identity.UpdatePreferredLanguageAsync(patient.UserId,
                request.PreferredLanguage, ct);
            if (languageUpdate.IsFailure)
            {
                return Result.Failure<PatientProfileDto>(languageUpdate.Error);
            }
        }

        return await assembler.BuildAsync(patient.Id, false, ct);
    }
}
