// =============================================================================
// File           : RefreshCookieOptions.cs
// Layer          : Api / Options
// Purpose        : Settings of the httpOnly cookie that carries the refresh token.
// Logic          : The cookie is always HttpOnly and Secure. SameSite is Strict by default (frontend and API on the
//                  same site); a frontend hosted on a different site (for example a Vercel preview) needs None, which
//                  is why it is configurable. The path is narrowed to the auth routes so the cookie is not sent with
//                  every API call.
// Implementation : Bound from the "RefreshCookie" configuration section and validated at start-up.
// =============================================================================
using System.ComponentModel.DataAnnotations;

namespace CardioSense.Api.Options;

/// <summary>Refresh-token cookie settings.</summary>
public sealed class RefreshCookieOptions
{
    /// <summary>Configuration section name.</summary>
    public const string SectionName = "RefreshCookie";

    /// <summary>Cookie name.</summary>
    [Required]
    public string Name { get; set; } = "cardiosense_refresh";

    /// <summary>URL path the browser sends the cookie to.</summary>
    [Required]
    public string Path { get; set; } = "/api/auth";

    /// <summary>SameSite attribute (Strict, Lax or None).</summary>
    public SameSiteMode SameSite { get; set; } = SameSiteMode.Strict;
}
