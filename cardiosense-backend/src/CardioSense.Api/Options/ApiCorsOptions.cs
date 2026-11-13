// =============================================================================
// File           : ApiCorsOptions.cs
// Layer          : Api / Options
// Purpose        : The browser origins that may call the API with credentials (the React web client).
// Logic          : The refresh token travels in an httpOnly cookie, so cross-origin calls must be credentialed and
//                  therefore cannot use a wildcard origin. An empty list makes start-up fail on purpose: a
//                  deployment that forgot to name its frontend should not silently accept every site.
// Implementation : Bound from the "Cors" configuration section and validated at start-up.
// =============================================================================
using System.ComponentModel.DataAnnotations;

namespace CardioSense.Api.Options;

/// <summary>Allowed browser origins.</summary>
public sealed class ApiCorsOptions
{
    /// <summary>Configuration section name.</summary>
    public const string SectionName = "Cors";

    /// <summary>Exact origins such as https://app.example.com (scheme + host + port, no trailing slash).</summary>
    [MinLength(1)]
    public string[] AllowedOrigins { get; set; } = [];
}
