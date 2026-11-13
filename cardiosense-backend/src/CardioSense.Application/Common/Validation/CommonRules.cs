// =============================================================================
// File           : CommonRules.cs
// Layer          : Application / Common/Validation
// Purpose        : Reusable FluentValidation rules shared by several validators (email, password, national
//                  id).
// Logic          : Mirrors the frontend Zod schemas exactly (email format, password min 8, national id = 14
//                  digits) so both sides reject the same input; defining each rule once keeps the validators
//                  DRY.
// Implementation : Extension methods on IRuleBuilder that chain the standard FluentValidation rules.
// =============================================================================
using FluentValidation;

namespace CardioSense.Application.Common.Validation;

/// <summary>Shared validation rules.</summary>
public static class CommonRules
{
    /// <summary>Maximum e-mail length (same as the frontend schema).</summary>
    public const int EmailMaxLength = 255;

    /// <summary>Minimum password length (same as the frontend schema).</summary>
    public const int PasswordMinLength = 8;

    /// <summary>Maximum password length (protects the hasher from huge inputs).</summary>
    public const int PasswordMaxLength = 128;

    /// <summary>Required, well-formed e-mail within the length limit.</summary>
    public static IRuleBuilderOptions<T, string> ValidEmail<T>(this IRuleBuilder<T, string> rule) =>
        rule.NotEmpty().MaximumLength(EmailMaxLength).EmailAddress();

    /// <summary>Required password within the allowed length range.</summary>
    public static IRuleBuilderOptions<T, string> ValidPassword<T>(this IRuleBuilder<T, string> rule) =>
        rule.NotEmpty().MinimumLength(PasswordMinLength).MaximumLength(PasswordMaxLength);

    /// <summary>
    /// Required national id made of exactly 14 ASCII digits (.NET \d would also accept Arabic-Indic digits).
    /// </summary>
    public static IRuleBuilderOptions<T, string> ValidNationalId<T>(this IRuleBuilder<T, string> rule) =>
        rule.NotEmpty().Matches("^[0-9]{14}$");
}
