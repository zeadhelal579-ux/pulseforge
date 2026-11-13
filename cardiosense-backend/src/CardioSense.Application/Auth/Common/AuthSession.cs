// =============================================================================
// File           : AuthSession.cs
// Layer          : Application / Auth/Common
// Purpose        : Internal result of any sign-in: access token, raw refresh token (for the cookie) and the
//                  user.
// Logic          : The Api layer splits it: the raw refresh token and its expiry become the httpOnly cookie,
//                  the rest becomes the JSON body via ToResponse()/ToRefreshResponse().
// Implementation : Immutable record with two projection helpers.
// =============================================================================
namespace CardioSense.Application.Auth.Common;

/// <summary>A freshly issued session.</summary>
public sealed record AuthSession(
    string AccessToken,
    string RawRefreshToken,
    DateTimeOffset RefreshTokenExpiresAt,
    AuthenticatedUserDto User)
{
    /// <summary>The JSON body for login/signup (no refresh token inside).</summary>
    public AuthResponse ToResponse() => new(AccessToken, User);

    /// <summary>The JSON body for token refresh.</summary>
    public RefreshResponse ToRefreshResponse() => new(AccessToken);
}
