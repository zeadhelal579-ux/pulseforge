// =============================================================================
// File           : RefreshTokenCommand.cs
// Layer          : Application / Auth/Commands/RefreshToken
// Purpose        : Request to exchange a refresh token for a new access token (and a rotated refresh token).
// Logic          : The Api reads the refresh token from the httpOnly cookie; the frontend calls this silently
//                  on page load and on 401.
// Implementation : Immutable record; the raw token is nullable because the cookie may be absent.
// =============================================================================
using CardioSense.Application.Auth.Common;
using CardioSense.Domain.Common;
using Mediator;

namespace CardioSense.Application.Auth.Commands.RefreshToken;

/// <summary>Refresh the session.</summary>
public sealed record RefreshTokenCommand(string? RawRefreshToken) : IRequest<Result<AuthSession>>;
