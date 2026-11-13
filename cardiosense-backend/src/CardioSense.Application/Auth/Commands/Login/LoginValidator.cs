// =============================================================================
// File           : LoginValidator.cs
// Layer          : Application / Auth/Commands/Login
// Purpose        : Validates the login request.
// Logic          : Only checks presence/shape (e-mail format, length caps) - password strength is a signup
//                  concern, not a login one.
// Implementation : FluentValidation validator discovered automatically by AddValidatorsFromAssembly.
// =============================================================================
using CardioSense.Application.Common.Validation;
using FluentValidation;

namespace CardioSense.Application.Auth.Commands.Login;

/// <summary>Rules for <see cref="LoginCommand"/>.</summary>
internal sealed class LoginValidator : AbstractValidator<LoginCommand>
{
    /// <summary>Declares the rules.</summary>
    public LoginValidator()
    {
        RuleFor(x => x.Email).ValidEmail();
        RuleFor(x => x.Password).NotEmpty().MaximumLength(CommonRules.PasswordMaxLength);
    }
}
