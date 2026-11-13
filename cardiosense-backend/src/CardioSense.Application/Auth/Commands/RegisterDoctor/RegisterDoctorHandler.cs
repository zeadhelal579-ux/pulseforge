// =============================================================================
// File           : RegisterDoctorHandler.cs
// Layer          : Application / Auth/Commands/RegisterDoctor
// Purpose        : Handles doctor signup: account + profile in one transaction, then a session.
// Logic          : A license number identifies one doctor, so a second signup with the same license is
//                  rejected with a Conflict.
// Implementation : Internal sealed handler; account creation is delegated to IIdentityService, profile
//                  creation to the Doctor entity.
// =============================================================================
using CardioSense.Application.Auth.Common;
using CardioSense.Application.Common.Errors;
using CardioSense.Application.Common.Interfaces;
using CardioSense.Application.Common.Models;
using CardioSense.Domain.Common;
using CardioSense.Domain.Entities;
using CardioSense.Domain.Enums;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace CardioSense.Application.Auth.Commands.RegisterDoctor;

/// <summary>Handles <see cref="RegisterDoctorCommand"/>.</summary>
internal sealed class RegisterDoctorHandler(
    IAppDbContext db, IIdentityService identity, SessionIssuer sessions, TimeProvider clock)
    : IRequestHandler<RegisterDoctorCommand, Result<AuthSession>>
{
    /// <summary>Creates the doctor and returns a signed-in session.</summary>
    public async ValueTask<Result<AuthSession>> Handle(RegisterDoctorCommand request, CancellationToken ct)
    {
        var license = request.LicenseNumber.Trim();
        if (await db.Doctors.AnyAsync(d => d.LicenseNumber == license, ct))
        {
            return Result.Failure<AuthSession>(ApplicationErrors.Doctor.LicenseTaken);
        }

        var now = clock.GetUtcNow();
        var email = request.Email.Trim();

        // One transaction covers the login account and the profile row.
        await using var transaction = await db.BeginTransactionAsync(ct);

        var created = await identity.CreateUserAsync(
            new NewUser(email, request.Password, UserRole.Doctor, request.PreferredLanguage), ct);
        if (created.IsFailure)
        {
            return Result.Failure<AuthSession>(created.Error);
        }

        db.Doctors.Add(Doctor.Create(
            created.Value,
            request.FullName,
            request.Specialty,
            license,
            request.Hospital,
            request.Department,
            request.Phone,
            now));

        await db.SaveChangesAsync(ct);
        await transaction.CommitAsync(ct);

        return await sessions.IssueAsync(new UserAccount(created.Value, email, UserRole.Doctor,
            request.PreferredLanguage), ct);
    }
}
