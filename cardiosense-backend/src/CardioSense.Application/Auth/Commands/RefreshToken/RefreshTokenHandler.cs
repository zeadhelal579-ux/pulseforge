// =============================================================================
// File           : RefreshTokenHandler.cs
// Layer          : Application / Auth/Commands/RefreshToken
// Purpose        : Handles token refresh with rotation and reuse detection.
// Logic          : A valid token is revoked the moment a new one is issued (rotation). A revoked token
//                  presented again after a short leeway was leaked or replayed, so every active token of that
//                  account is revoked and the user must sign in again. Inside the leeway it is a harmless
//                  concurrent retry (two tabs, React StrictMode) and is accepted.
// Implementation : Internal sealed handler. Note: this namespace shares its name with the RefreshToken
//                  entity, so the entity type is never referenced by name here - only through
//                  db.RefreshTokens.
// =============================================================================
using CardioSense.Application.Auth.Common;
using CardioSense.Application.Common.Errors;
using CardioSense.Application.Common.Interfaces;
using CardioSense.Domain.Common;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace CardioSense.Application.Auth.Commands.RefreshToken;

/// <summary>Handles <see cref="RefreshTokenCommand"/>.</summary>
internal sealed class RefreshTokenHandler(
    IAppDbContext db, IIdentityService identity, ITokenService tokens, SessionIssuer sessions, TimeProvider clock)
    : IRequestHandler<RefreshTokenCommand, Result<AuthSession>>
{
    /// <summary>
    /// How long a just-rotated token is still accepted (simultaneous refresh calls carry the same cookie).
    /// </summary>
    private static readonly TimeSpan ReuseLeeway = TimeSpan.FromSeconds(10);

    /// <summary>Validates the presented token, rotates it and issues a new session.</summary>
    public async ValueTask<Result<AuthSession>> Handle(RefreshTokenCommand request, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(request.RawRefreshToken))
        {
            return Result.Failure<AuthSession>(ApplicationErrors.Auth.InvalidRefreshToken);
        }

        var now = clock.GetUtcNow();
        var hash = tokens.HashRefreshToken(request.RawRefreshToken);
        var stored = await db.RefreshTokens.FirstOrDefaultAsync(t => t.TokenHash == hash, ct);

        if (stored is null)
        {
            return Result.Failure<AuthSession>(ApplicationErrors.Auth.InvalidRefreshToken);
        }

        // A revoked token presented after the leeway was stolen or replayed: kill every session.
        if (stored.IsReplay(now, ReuseLeeway))
        {
            await RevokeAllActiveAsync(stored.UserId, now, ct);
            return Result.Failure<AuthSession>(ApplicationErrors.Auth.InvalidRefreshToken);
        }

        if (!stored.IsUsable(now, ReuseLeeway))
        {
            return Result.Failure<AuthSession>(ApplicationErrors.Auth.InvalidRefreshToken);
        }

        var account = await identity.FindByIdAsync(stored.UserId, ct);
        if (account is null)
        {
            return Result.Failure<AuthSession>(ApplicationErrors.Auth.InvalidRefreshToken);
        }

        // Rotation: the old token dies here (idempotent inside the leeway); IssueAsync saves it with the new token.
        stored.Revoke(now);
        return await sessions.IssueAsync(account, ct);
    }

    /// <summary>Revokes every still-active refresh token of the account.</summary>
    private async Task RevokeAllActiveAsync(Guid userId, DateTimeOffset now, CancellationToken ct)
    {
        var active = await db.RefreshTokens
            .Where(t => t.UserId == userId && t.RevokedAt == null)
            .ToListAsync(ct);

        foreach (var token in active)
        {
            token.Revoke(now);
        }

        await db.SaveChangesAsync(ct);
    }
}
