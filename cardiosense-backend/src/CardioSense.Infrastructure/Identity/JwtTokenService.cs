// =============================================================================
// File           : JwtTokenService.cs
// Layer          : Infrastructure / Identity
// Purpose        : Implements ITokenService - signed JWT access tokens and opaque refresh tokens.
// Logic          : The access token is a short-lived HS256 JWT carrying sub, role and (per role) the patient or doctor
//                  profile id. The refresh token is 64 random bytes (Base64Url) that only ever lives in the httpOnly
//                  cookie; the database stores its SHA-256 hash, so a stolen backup cannot be replayed.
// Implementation : Singleton (stateless). The signing credentials are built once from the options; the hash is
//                  deterministic so a presented cookie can be looked up by hash.
// =============================================================================
using System.Buffers.Text;
using System.Security.Cryptography;
using System.Text;
using CardioSense.Application.Common.Interfaces;
using CardioSense.Application.Common.Models;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;

namespace CardioSense.Infrastructure.Identity;

/// <summary>Creates and hashes tokens.</summary>
internal sealed class JwtTokenService(IOptions<JwtOptions> options, TimeProvider clock) : ITokenService
{
    /// <summary>Number of random bytes in a refresh token (512 bits).</summary>
    private const int RefreshTokenBytes = 64;

    /// <summary>Thread-safe handler that writes the JWT.</summary>
    private readonly JsonWebTokenHandler _handler = new();

    /// <summary>Signing key and algorithm, created once.</summary>
    private readonly SigningCredentials _credentials = new(
        new SymmetricSecurityKey(Encoding.UTF8.GetBytes(options.Value.SigningKey)),
        SecurityAlgorithms.HmacSha256);

    /// <inheritdoc />
    public AccessToken CreateAccessToken(AccessTokenRequest request)
    {
        var settings = options.Value;
        var now = clock.GetUtcNow();
        var expiresAt = now.AddMinutes(settings.AccessTokenMinutes);

        var claims = new Dictionary<string, object>
        {
            [JwtClaimNames.Subject] = request.UserId.ToString(),
            [JwtClaimNames.Role] = request.Role.ToString()
        };

        if (request.PatientId is { } patientId)
        {
            claims[JwtClaimNames.PatientId] = patientId.ToString();
        }

        if (request.DoctorId is { } doctorId)
        {
            claims[JwtClaimNames.DoctorId] = doctorId.ToString();
        }

        var descriptor = new SecurityTokenDescriptor
        {
            Issuer = settings.Issuer,
            Audience = settings.Audience,
            Claims = claims,
            NotBefore = now.UtcDateTime,
            IssuedAt = now.UtcDateTime,
            Expires = expiresAt.UtcDateTime,
            SigningCredentials = _credentials
        };

        return new AccessToken(_handler.CreateToken(descriptor), expiresAt);
    }

    /// <inheritdoc />
    public GeneratedRefreshToken GenerateRefreshToken(DateTimeOffset now)
    {
        var rawValue = Base64Url.EncodeToString(RandomNumberGenerator.GetBytes(RefreshTokenBytes));
        var expiresAt = now.AddDays(options.Value.RefreshTokenDays);
        return new GeneratedRefreshToken(rawValue, HashRefreshToken(rawValue), expiresAt);
    }

    /// <inheritdoc />
    public string HashRefreshToken(string rawValue) =>
        Convert.ToBase64String(SHA256.HashData(Encoding.UTF8.GetBytes(rawValue)));
}
