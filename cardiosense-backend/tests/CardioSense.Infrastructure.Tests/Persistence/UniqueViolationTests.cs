// =============================================================================
// File           : UniqueViolationTests.cs
// Layer          : Tests / Persistence
// Purpose        : Verifies that only genuine duplicate-key failures are reported as unique violations.
// Logic          : The Api turns a unique violation into 409 Conflict and everything else into 500, so the detector
//                  must not claim ordinary database or application errors. (A real duplicate-key SqlException cannot
//                  be constructed outside the SQL client, so the positive case is covered by the integration tests.)
// Implementation : Plain xUnit facts over UniqueViolation.IsCausedBy.
// =============================================================================
using CardioSense.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CardioSense.Infrastructure.Tests.Persistence;

/// <summary>Tests for UniqueViolation.</summary>
public sealed class UniqueViolationTests
{
    /// <summary>An arbitrary exception is not a unique violation.</summary>
    [Fact]
    public void An_ordinary_exception_is_not_a_unique_violation() =>
        Assert.False(UniqueViolation.IsCausedBy(new InvalidOperationException("boom")));

    /// <summary>A failed save with an unrelated cause is not a unique violation.</summary>
    [Fact]
    public void A_failed_save_with_another_cause_is_not_a_unique_violation() =>
        Assert.False(UniqueViolation.IsCausedBy(new DbUpdateException("save failed", new TimeoutException())));

    /// <summary>A failed save with no inner exception is not a unique violation.</summary>
    [Fact]
    public void A_failed_save_without_an_inner_exception_is_not_a_unique_violation() =>
        Assert.False(UniqueViolation.IsCausedBy(new DbUpdateException("save failed")));
}
