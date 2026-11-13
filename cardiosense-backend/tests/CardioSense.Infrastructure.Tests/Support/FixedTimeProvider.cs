// =============================================================================
// File           : FixedTimeProvider.cs
// Layer          : Tests / Support
// Purpose        : A clock the test controls.
// Logic          : Token expiry is computed from TimeProvider, so a fixed "now" makes the expected expiry exact.
// Implementation : Overrides GetUtcNow only.
// =============================================================================
namespace CardioSense.Infrastructure.Tests.Support;

/// <summary>A time provider that always returns the same instant.</summary>
public sealed class FixedTimeProvider(DateTimeOffset now) : TimeProvider
{
    /// <summary>Returns the fixed instant.</summary>
    public override DateTimeOffset GetUtcNow() => now;
}
