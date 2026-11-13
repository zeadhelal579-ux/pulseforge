// =============================================================================
// File           : IdentityErrorsTests.cs
// Layer          : Tests / Identity
// Purpose        : Verifies how ASP.NET Core Identity failures become application errors.
// Logic          : The frontend reacts to the category (a taken e-mail is a conflict, a weak password is a validation
//                  error on the password field), so the mapping from Identity's error codes must stay exact.
// Implementation : Feeds IdentityError values to IdentityErrors.From and checks the resulting Error.
// =============================================================================
using CardioSense.Domain.Common;
using CardioSense.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;

namespace CardioSense.Infrastructure.Tests.Identity;

/// <summary>Tests for IdentityErrors.</summary>
public sealed class IdentityErrorsTests
{
    /// <summary>Builds an Identity error.</summary>
    private static IdentityError Failure(string code, string description = "Problem.") =>
        new() { Code = code, Description = description };

    /// <summary>A duplicate e-mail or user name is a conflict.</summary>
    [Theory]
    [InlineData("DuplicateEmail")]
    [InlineData("DuplicateUserName")]
    public void A_duplicate_email_is_a_conflict(string code)
    {
        var error = IdentityErrors.From([Failure(code)]);

        Assert.Equal(ErrorType.Conflict, error.Type);
        Assert.Equal("Identity.EmailTaken", error.Code);
    }

    /// <summary>Password rules are reported on the password field.</summary>
    [Fact]
    public void A_weak_password_is_a_validation_error_on_the_password_field()
    {
        var error = IdentityErrors.From([Failure("PasswordTooShort", "Passwords must be at least 8 characters.")]);

        Assert.Equal(ErrorType.Validation, error.Type);
        var fields = error.ValidationErrors;
        Assert.NotNull(fields);
        Assert.Equal("Passwords must be at least 8 characters.", Assert.Single(fields["password"]));
    }

    /// <summary>An invalid e-mail is reported on the email field.</summary>
    [Fact]
    public void An_invalid_email_is_a_validation_error_on_the_email_field()
    {
        var error = IdentityErrors.From([Failure("InvalidEmail")]);

        Assert.Equal(ErrorType.Validation, error.Type);
        Assert.NotNull(error.ValidationErrors);
        Assert.Contains("email", error.ValidationErrors.Keys);
    }

    /// <summary>Anything else is a general failure that keeps Identity's description.</summary>
    [Fact]
    public void An_unrecognised_error_is_a_general_failure()
    {
        var error = IdentityErrors.From([Failure("ConcurrencyFailure", "Optimistic concurrency failure.")]);

        Assert.Equal(ErrorType.Failure, error.Type);
        Assert.Equal("Identity.OperationFailed", error.Code);
        Assert.Contains("Optimistic concurrency failure.", error.Description);
    }
}
