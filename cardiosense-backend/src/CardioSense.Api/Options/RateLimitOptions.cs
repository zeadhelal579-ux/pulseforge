// =============================================================================
// File           : RateLimitOptions.cs
// Layer          : Api / Options
// Purpose        : How many login/signup attempts one client address may make per time window.
// Logic          : Password guessing and mass sign-ups are the realistic attacks on a public auth endpoint; a fixed
//                  window per client address slows them down without affecting normal use.
// Implementation : Bound from the "RateLimiting" configuration section and validated at start-up.
// =============================================================================
using System.ComponentModel.DataAnnotations;

namespace CardioSense.Api.Options;

/// <summary>Rate-limit settings for the auth endpoints.</summary>
public sealed class RateLimitOptions
{
    /// <summary>Configuration section name.</summary>
    public const string SectionName = "RateLimiting";

    /// <summary>Requests allowed per client address in one window.</summary>
    [Range(1, 1000)]
    public int AuthPermitLimit { get; set; } = 10;

    /// <summary>Window length in seconds.</summary>
    [Range(1, 3600)]
    public int AuthWindowSeconds { get; set; } = 60;
}
