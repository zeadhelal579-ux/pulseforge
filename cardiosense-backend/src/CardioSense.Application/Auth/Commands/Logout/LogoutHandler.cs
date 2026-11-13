// =============================================================================
// File           : LogoutHandler.cs
// Layer          : Application / Auth/Commands/Logout
// Purpose        : Handles a logout by revoking the presented refresh token in the database.
// Logic          : Idempotent: no token, unknown token or already-revoked token all succeed, so logging out
//                  twice never errors.
// Implementation : Internal sealed handler; looks the token up by its hash and marks it revoked.
// =============================================================================
using CardioSense.Application.Common.Interfaces;
using CardioSense.Domain.Common;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace CardioSense.Application.Auth.Commands.Logout;

/// <summary>Handles <see cref="LogoutCommand"/>.</summary>
internal sealed class LogoutHandler(IAppDbContext db, ITokenService tokens, TimeProvider clock)
    : IRequestHandler<LogoutCommand, Result>
{
    /// <summary>Revokes the refresh token when one was presented.</summary>
    public async ValueTask<Result> Handle(LogoutCommand request, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(request.RawRefreshToken))
        {
            return Result.Success();
        }

        var hash = tokens.HashRefreshToken(request.RawRefreshToken);
        var stored = await db.RefreshTokens.FirstOrDefaultAsync(t => t.TokenHash == hash, ct);

        if (stored is not null)
        {
            stored.Revoke(clock.GetUtcNow());
            await db.SaveChangesAsync(ct);
        }

        return Result.Success();
    }
}
