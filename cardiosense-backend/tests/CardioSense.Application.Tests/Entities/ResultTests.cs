// =============================================================================
// File           : ResultTests.cs
// Layer          : Tests / Entities
// Purpose        : Verifies the Result type that every use case returns instead of throwing for expected failures.
// Logic          : A failed result must never hand out a value, and a result cannot be both successful and carry an
//                  error - mistakes the type makes impossible, so no caller can read a missing value by accident.
// Implementation : Plain xUnit facts; no database.
// =============================================================================
using CardioSense.Domain.Common;

namespace CardioSense.Application.Tests.Entities;

/// <summary>Tests for Result and Result of T.</summary>
public sealed class ResultTests
{
    /// <summary>A successful result carries its value and no error.</summary>
    [Fact]
    public void A_successful_result_has_its_value()
    {
        var result = Result.Success(42);

        Assert.True(result.IsSuccess);
        Assert.Equal(42, result.Value);
        Assert.Equal(Error.None, result.Error);
    }

    /// <summary>Reading the value of a failed result throws instead of returning a default.</summary>
    [Fact]
    public void Reading_the_value_of_a_failed_result_throws()
    {
        var result = Result.Failure<int>(Error.NotFound("X.NotFound", "Missing."));

        Assert.True(result.IsFailure);
        Assert.Throws<InvalidOperationException>(() => result.Value);
    }

    /// <summary>The error category is preserved so the Api can pick the HTTP status.</summary>
    [Fact]
    public void A_failure_keeps_its_error_type()
    {
        var result = Result.Failure(Error.Conflict("X.Conflict", "Taken."));

        Assert.Equal(ErrorType.Conflict, result.Error.Type);
        Assert.Equal("X.Conflict", result.Error.Code);
    }
}
