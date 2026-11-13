// =============================================================================
// File           : FixedTimeProvider.cs
// Layer          : Tests / Support
// Purpose        : A clock the test controls.
// Logic          : Handlers read the time from TimeProvider, so a test that fixes "now" can check expiry windows,
//                  future-timestamp rejection and "live device" rules without sleeping.
// Implementation : Overrides GetUtcNow only; Advance moves the clock forward.
// =============================================================================
namespace CardioSense.Application.Tests.Support;

/// <summary>A time provider that returns a fixed, adjustable instant.</summary>
public sealed class FixedTimeProvider(DateTimeOffset start) : TimeProvider
{
    /// <summary>The instant currently reported as "now".</summary>
    private DateTimeOffset _now = start;

    /// <summary>Returns the fixed instant.</summary>
    public override DateTimeOffset GetUtcNow() => _now;

    /// <summary>Moves the clock forward.</summary>
    public void Advance(TimeSpan by) => _now += by;
}
