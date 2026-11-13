// =============================================================================
// File           : SessionIssuer.cs
// Layer          : Application / Auth/Common
// Purpose        : Issues a session for an account: finds the profile, creates the access token, creates and
//                  stores the refresh token.
// Logic          : Login, both signups and token refresh all end the same way, so the steps live here once
//                  (DRY). The refresh token is stored hashed; the raw value is returned only so the Api can
//                  put it in the cookie.
// Implementation : Internal scoped service; saves changes itself, so callers may revoke an old token before
//                  calling it and both changes are persisted together.
// =============================================================================
using CardioSense.Application.Common.Errors;
using CardioSense.Application.Common.Interfaces;
using CardioSense.Application.Common.Models;
using CardioSense.Domain.Common;
using CardioSense.Domain.Entities;
using CardioSense.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace CardioSense.Application.Auth.Common;

/// <summary>Creates authenticated sessions.</summary>
internal sealed class SessionIssuer(IAppDbContext db, ITokenService tokens, TimeProvider clock)
{
    /// <summary>Issues access + refresh tokens for the account and returns the session.</summary>
    public async Task<Result<AuthSession>> IssueAsync(UserAccount account, CancellationToken ct)
    {
        var profile = await FindProfileAsync(account, ct);
        if (profile is null)
        {
            return Result.Failure<AuthSession>(ApplicationErrors.Auth.ProfileMissing);
        }

        var now = clock.GetUtcNow();
        var accessToken = tokens.CreateAccessToken(
            new AccessTokenRequest(account.UserId, account.Role, profile.PatientId, profile.DoctorId));

        var refresh = tokens.GenerateRefreshToken(now);
        db.RefreshTokens.Add(RefreshToken.Create(account.UserId, refresh.Hash, refresh.ExpiresAt, now));
        await db.SaveChangesAsync(ct);

        var user = new AuthenticatedUserDto(
            account.UserId,
            account.Role,
            profile.FullName,
            account.Email,
            account.PreferredLanguage,
            profile.PatientId,
            profile.DoctorId,
            profile.Specialty);

        return Result.Success(new AuthSession(accessToken.Value, refresh.RawValue, refresh.ExpiresAt, user));
    }

    /// <summary>Loads the patient or doctor profile that belongs to the account.</summary>
    private Task<SessionProfile?> FindProfileAsync(UserAccount account, CancellationToken ct) =>
        account.Role == UserRole.Patient
            ? db.Patients.AsNoTracking()
                .Where(p => p.UserId == account.UserId)
                .Select(p => new SessionProfile(p.FullName, p.Id, null, null))
                .FirstOrDefaultAsync(ct)
            : db.Doctors.AsNoTracking()
                .Where(d => d.UserId == account.UserId)
                .Select(d => new SessionProfile(d.FullName, null, d.Id, d.Specialty))
                .FirstOrDefaultAsync(ct);

    /// <summary>The few profile fields a session needs.</summary>
    private sealed record SessionProfile(string FullName, Guid? PatientId, Guid? DoctorId, string? Specialty);
}
