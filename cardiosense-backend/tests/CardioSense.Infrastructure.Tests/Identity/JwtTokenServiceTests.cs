// =============================================================================
// File           : JwtTokenServiceTests.cs
// Layer          : Tests / Identity
// Purpose        : Verifies access-token contents and refresh-token generation.
// Logic          : The access token is what every authorization decision reads, so its claims must be exactly what the
//                  Api's bearer setup expects (sub, role, patientId/doctorId) and its signature must hold only for the
//                  configured key. The refresh token must be unpredictable, and what the database stores is a
//                  deterministic hash (so it can be looked up) that cannot be turned back into the cookie value.
// Implementation : Tokens are read back with the same JWT library the Api validates with.
// =============================================================================
using System.Text;
using CardioSense.Application.Common.Models;
using CardioSense.Domain.Enums;
using CardioSense.Infrastructure.Identity;
using CardioSense.Infrastructure.Tests.Support;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;

namespace CardioSense.Infrastructure.Tests.Identity;

/// <summary>Tests for JwtTokenService.</summary>
public sealed class JwtTokenServiceTests
{
    /// <summary>The signing key used by the tests (32 or more characters is required).</summary>
    private const string SigningKey = "unit-test-signing-key-0123456789-abcdef";

    /// <summary>A different key, to prove tokens only validate with the configured one.</summary>
    private const string OtherSigningKey = "another-signing-key-0123456789-abcdef";

    /// <summary>The instant the service believes it is.</summary>
    private static readonly DateTimeOffset Now = new(2026, 11, 1, 12, 0, 0, TimeSpan.Zero);

    /// <summary>The token settings used by the tests.</summary>
    private static readonly JwtOptions Settings = new()
    {
        Issuer = "CardioSense",
        Audience = "CardioSense.Web",
        SigningKey = SigningKey,
        AccessTokenMinutes = 15,
        RefreshTokenDays = 7
    };

    /// <summary>Creates the service under test.</summary>
    private static JwtTokenService Create() =>
        new(new OptionsWrapper<JwtOptions>(Settings), new FixedTimeProvider(Now));

    /// <summary>A token request for a patient with the given profile id.</summary>
    private static AccessTokenRequest AsPatient(Guid patientId) =>
        new(Guid.NewGuid(), UserRole.Patient, patientId, null);

    /// <summary>Reads the claim of the given type, or null.</summary>
    private static string? ClaimOf(JsonWebToken token, string type) =>
        token.Claims.FirstOrDefault(claim => claim.Type == type)?.Value;

    /// <summary>A doctor's token carries sub, role and doctorId - and no patientId.</summary>
    [Fact]
    public void A_doctor_token_carries_the_doctor_claims()
    {
        var userId = Guid.NewGuid();
        var doctorId = Guid.NewGuid();

        var token = Create().CreateAccessToken(new AccessTokenRequest(userId, UserRole.Doctor, null, doctorId));

        var jwt = new JsonWebTokenHandler().ReadJsonWebToken(token.Value);
        Assert.Equal(userId.ToString(), ClaimOf(jwt, JwtClaimNames.Subject));
        Assert.Equal("Doctor", ClaimOf(jwt, JwtClaimNames.Role));
        Assert.Equal(doctorId.ToString(), ClaimOf(jwt, JwtClaimNames.DoctorId));
        Assert.Null(ClaimOf(jwt, JwtClaimNames.PatientId));
        Assert.Equal("CardioSense", jwt.Issuer);
        Assert.Contains("CardioSense.Web", jwt.Audiences);
    }

    /// <summary>A patient's token carries patientId - and no doctorId.</summary>
    [Fact]
    public void A_patient_token_carries_the_patient_claims()
    {
        var patientId = Guid.NewGuid();

        var token = Create().CreateAccessToken(AsPatient(patientId));

        var jwt = new JsonWebTokenHandler().ReadJsonWebToken(token.Value);
        Assert.Equal("Patient", ClaimOf(jwt, JwtClaimNames.Role));
        Assert.Equal(patientId.ToString(), ClaimOf(jwt, JwtClaimNames.PatientId));
        Assert.Null(ClaimOf(jwt, JwtClaimNames.DoctorId));
    }

    /// <summary>The token expires after the configured number of minutes.</summary>
    [Fact]
    public void The_token_expires_after_the_configured_minutes()
    {
        var token = Create().CreateAccessToken(AsPatient(Guid.NewGuid()));

        Assert.Equal(Now.AddMinutes(15), token.ExpiresAt);
    }

    /// <summary>The signature is valid for the configured key and invalid for any other.</summary>
    [Fact]
    public async Task The_signature_is_valid_only_for_the_configured_key()
    {
        var token = Create().CreateAccessToken(AsPatient(Guid.NewGuid()));
        var handler = new JsonWebTokenHandler();

        var withRightKey = await handler.ValidateTokenAsync(token.Value, ValidationFor(SigningKey));
        var withWrongKey = await handler.ValidateTokenAsync(token.Value, ValidationFor(OtherSigningKey));

        Assert.True(withRightKey.IsValid);
        Assert.False(withWrongKey.IsValid);
    }

    /// <summary>Refresh tokens are long, URL-safe, unique, and expire after the configured days.</summary>
    [Fact]
    public void Refresh_tokens_are_long_url_safe_and_unique()
    {
        var service = Create();

        var first = service.GenerateRefreshToken(Now);
        var second = service.GenerateRefreshToken(Now);

        Assert.Equal(86, first.RawValue.Length);
        Assert.DoesNotContain("+", first.RawValue);
        Assert.DoesNotContain("/", first.RawValue);
        Assert.DoesNotContain("=", first.RawValue);
        Assert.NotEqual(first.RawValue, second.RawValue);
        Assert.Equal(Now.AddDays(7), first.ExpiresAt);
    }

    /// <summary>The stored hash is deterministic and is not the cookie value.</summary>
    [Fact]
    public void The_stored_hash_is_deterministic_and_not_the_raw_value()
    {
        var service = Create();
        var token = service.GenerateRefreshToken(Now);

        Assert.Equal(token.Hash, service.HashRefreshToken(token.RawValue));
        Assert.NotEqual(token.RawValue, token.Hash);
        Assert.Equal(44, token.Hash.Length);
    }

    /// <summary>Validation parameters equal to what the Api uses, for a given key.</summary>
    private static TokenValidationParameters ValidationFor(string key) => new()
    {
        ValidIssuer = Settings.Issuer,
        ValidAudience = Settings.Audience,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key)),
        ValidateLifetime = false
    };
}
