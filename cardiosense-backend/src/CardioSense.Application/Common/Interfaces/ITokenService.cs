// =============================================================================
// File           : ITokenService.cs
// Layer          : Application / Common/Interfaces
// Purpose        : Token contract: create JWT access tokens, generate refresh tokens and hash them for
//                  storage.
// Logic          : Access tokens are short-lived and carry sub/role/patientId/doctorId; refresh tokens are
//                  random, returned once for the httpOnly cookie and stored only as a hash.
// Implementation : Implemented by JwtTokenService in Infrastructure; HashRefreshToken must be deterministic
//                  (same input, same hash).
// =============================================================================
using CardioSense.Application.Common.Models;

namespace CardioSense.Application.Common.Interfaces;

/// <summary>Issues and hashes tokens.</summary>
public interface ITokenService
{
    /// <summary>Creates a signed JWT access token.</summary>
    AccessToken CreateAccessToken(AccessTokenRequest request);

    /// <summary>Generates a new random refresh token (raw value, hash and expiry).</summary>
    GeneratedRefreshToken GenerateRefreshToken(DateTimeOffset now);

    /// <summary>Hashes a raw refresh token deterministically so it can be looked up in the database.</summary>
    string HashRefreshToken(string rawValue);
}
