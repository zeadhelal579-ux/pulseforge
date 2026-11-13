// =============================================================================
// File           : JwtOptions.cs
// Layer          : Infrastructure / Identity
// Purpose        : Strongly-typed settings of the JWT access token and the refresh token lifetime.
// Logic          : Bound from the "Jwt" configuration section and validated when the application starts, so a missing
//                  or too-short signing key stops the host immediately instead of failing on the first login.
// Implementation : Plain options class with data-annotation rules; the same values are used by the Api to validate
//                  incoming tokens.
// =============================================================================
using System.ComponentModel.DataAnnotations;

namespace CardioSense.Infrastructure.Identity;

/// <summary>Settings of the token service.</summary>
public sealed class JwtOptions
{
    /// <summary>Configuration section name.</summary>
    public const string SectionName = "Jwt";

    /// <summary>Shortest accepted signing key (HS256 needs at least 256 bits).</summary>
    public const int MinSigningKeyLength = 32;

    /// <summary>Who issues the token (the 'iss' claim).</summary>
    [Required]
    public string Issuer { get; set; } = string.Empty;

    /// <summary>Who the token is for (the 'aud' claim).</summary>
    [Required]
    public string Audience { get; set; } = string.Empty;

    /// <summary>Secret that signs tokens; comes from User Secrets or the environment, never from git.</summary>
    [Required]
    [MinLength(MinSigningKeyLength)]
    public string SigningKey { get; set; } = string.Empty;

    /// <summary>Lifetime of an access token in minutes (short on purpose).</summary>
    [Range(1, 60)]
    public int AccessTokenMinutes { get; set; } = 15;

    /// <summary>Lifetime of a refresh token in days.</summary>
    [Range(1, 90)]
    public int RefreshTokenDays { get; set; } = 7;
}
