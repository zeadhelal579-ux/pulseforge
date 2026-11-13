// =============================================================================
// File           : IdentityErrors.cs
// Layer          : Infrastructure / Identity
// Purpose        : Catalogue of account-level failures plus the translation of ASP.NET Identity errors into Errors.
// Logic          : Identity reports problems as IdentityError codes; the rest of the system speaks Result/Error, so
//                  every code is mapped here once: duplicate e-mail -> Conflict, password/e-mail format ->
//                  Validation (with the field name the frontend binds to), anything else -> Failure.
// Implementation : Internal static class; fixed errors are static readonly fields, the mapping is one method.
// =============================================================================
using CardioSense.Domain.Common;
using Microsoft.AspNetCore.Identity;

namespace CardioSense.Infrastructure.Identity;

/// <summary>Errors raised by the identity service.</summary>
internal static class IdentityErrors
{
    /// <summary>Another account already uses this e-mail.</summary>
    public static readonly Error EmailTaken = Error.Conflict(
        "Identity.EmailTaken", "An account with this e-mail already exists.");

    /// <summary>The account is locked after too many failed sign-in attempts.</summary>
    public static readonly Error LockedOut = Error.Forbidden(
        "Identity.LockedOut", "The account is temporarily locked after too many failed sign-in attempts.");

    /// <summary>The account does not exist.</summary>
    public static readonly Error UserNotFound = Error.NotFound(
        "Identity.UserNotFound", "The account was not found.");

    /// <summary>Converts the errors reported by Identity into one Error.</summary>
    public static Error From(IEnumerable<IdentityError> identityErrors)
    {
        var errors = identityErrors.ToList();

        if (errors.Any(error => error.Code is "DuplicateEmail" or "DuplicateUserName"))
        {
            return EmailTaken;
        }

        var fieldErrors = errors
            .Select(error => (Field: FieldOf(error.Code), error.Description))
            .Where(entry => entry.Field.Length > 0)
            .GroupBy(entry => entry.Field, entry => entry.Description)
            .ToDictionary(group => group.Key, group => group.ToArray());

        return fieldErrors.Count > 0
            ? Error.Validation("Identity.InvalidInput", "The account data is invalid.", fieldErrors)
            : Error.Failure("Identity.OperationFailed", string.Join(" ", errors.Select(error => error.Description)));
    }

    /// <summary>The frontend field an Identity error code belongs to; empty when it belongs to none.</summary>
    private static string FieldOf(string code) => code switch
    {
        _ when code.StartsWith("Password", StringComparison.Ordinal) => "password",
        "InvalidEmail" or "InvalidUserName" => "email",
        _ => string.Empty
    };
}
