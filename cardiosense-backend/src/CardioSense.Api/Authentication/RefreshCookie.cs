// =============================================================================
// File           : RefreshCookie.cs
// Layer          : Api / Authentication
// Purpose        : Writes, reads and clears the httpOnly cookie that carries the refresh token.
// Logic          : The refresh token never appears in a JSON body or in JavaScript-readable storage (the frontend
//                  keeps only the short-lived access token in memory). Every attribute that matters for security is
//                  fixed here: HttpOnly and Secure always on, narrow path, expiry equal to the stored token's expiry.
//                  Clearing must repeat the same path and SameSite or the browser keeps the old cookie.
// Implementation : Singleton over the validated RefreshCookieOptions; used by the auth endpoints only.
// =============================================================================
using CardioSense.Api.Options;
using Microsoft.Extensions.Options;

namespace CardioSense.Api.Authentication;

/// <summary>Reads and writes the refresh-token cookie.</summary>
internal sealed class RefreshCookie(IOptions<RefreshCookieOptions> options)
{
    /// <summary>Stores the raw refresh token in the response cookie until it expires.</summary>
    public void Write(HttpResponse response, string rawToken, DateTimeOffset expiresAt) =>
        response.Cookies.Append(options.Value.Name, rawToken, Describe(expiresAt));

    /// <summary>Returns the raw refresh token sent by the browser, or null when there is none.</summary>
    public string? Read(HttpRequest request) =>
        request.Cookies.TryGetValue(options.Value.Name, out var rawToken) ? rawToken : null;

    /// <summary>Tells the browser to delete the cookie.</summary>
    public void Clear(HttpResponse response) => response.Cookies.Delete(options.Value.Name, Describe(null));

    /// <summary>Builds the cookie attributes shared by writing and clearing.</summary>
    private CookieOptions Describe(DateTimeOffset? expiresAt) => new()
    {
        HttpOnly = true,
        Secure = true,
        SameSite = options.Value.SameSite,
        Path = options.Value.Path,
        Expires = expiresAt,
        IsEssential = true
    };
}
