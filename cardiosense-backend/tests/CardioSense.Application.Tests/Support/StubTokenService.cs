// =============================================================================
// File           : StubTokenService.cs
// Layer          : Tests / Support
// Purpose        : ITokenService with predictable, readable tokens.
// Logic          : Real JWT signing is an Infrastructure concern (tested there). Here a token only has to be a
//                  stable string, and the hash only has to be deterministic so a stored token can be found again.
// Implementation : Sequential raw tokens and a "hash-of-" prefix as the hash.
// =============================================================================
using CardioSense.Application.Common.Interfaces;
using CardioSense.Application.Common.Models;

namespace CardioSense.Application.Tests.Support;

/// <summary>Predictable token generation.</summary>
public sealed class StubTokenService : ITokenService
{
    /// <summary>Number of refresh tokens generated so far.</summary>
    private int _issued;

    /// <inheritdoc />
    public AccessToken CreateAccessToken(AccessTokenRequest request) =>
        new($"access-{request.UserId}", DateTimeOffset.UnixEpoch.AddYears(100));

    /// <inheritdoc />
    public GeneratedRefreshToken GenerateRefreshToken(DateTimeOffset now)
    {
        var sequence = Interlocked.Increment(ref _issued);
        var raw = $"raw-{sequence}";
        return new GeneratedRefreshToken(raw, HashRefreshToken(raw), now.AddDays(7));
    }

    /// <inheritdoc />
    public string HashRefreshToken(string rawValue) => $"hash-of-{rawValue}";
}
