// =============================================================================
// File           : Error.cs
// Layer          : Domain / Common
// Purpose        : Immutable description of why an operation failed (code + message + category).
// Logic          : Expected failures travel as values (Result) instead of exceptions; the category lets the
//                  Api layer translate any Error into an RFC 7807 ProblemDetails response.
// Implementation : Sealed record with one factory per category. Validation errors can also carry field-level
//                  messages (camelCase field name -> messages) for the frontend.
// =============================================================================
namespace CardioSense.Domain.Common;

/// <summary>Describes a failure; compared by value.</summary>
public sealed record Error(string Code, string Description, ErrorType Type)
{
    /// <summary>Sentinel carried by successful results (no error).</summary>
    public static readonly Error None = new(string.Empty, string.Empty, ErrorType.Failure);

    /// <summary>Field-level messages for validation errors; null for every other category.</summary>
    public IReadOnlyDictionary<string, string[]>? ValidationErrors { get; init; }

    /// <summary>Creates a business-rule failure.</summary>
    public static Error Failure(string code, string description) =>
        new(code, description, ErrorType.Failure);

    /// <summary>Creates a validation failure, optionally with per-field messages.</summary>
    public static Error Validation(
        string code, string description, IReadOnlyDictionary<string, string[]>? fieldErrors = null) =>
        new(code, description, ErrorType.Validation) { ValidationErrors = fieldErrors };

    /// <summary>Creates an authentication failure.</summary>
    public static Error Unauthorized(string code, string description) =>
        new(code, description, ErrorType.Unauthorized);

    /// <summary>Creates an authorization failure.</summary>
    public static Error Forbidden(string code, string description) =>
        new(code, description, ErrorType.Forbidden);

    /// <summary>Creates a missing-resource failure.</summary>
    public static Error NotFound(string code, string description) =>
        new(code, description, ErrorType.NotFound);

    /// <summary>Creates a state-conflict failure.</summary>
    public static Error Conflict(string code, string description) =>
        new(code, description, ErrorType.Conflict);

    /// <summary>Creates a dependency-unavailable failure.</summary>
    public static Error Unavailable(string code, string description) =>
        new(code, description, ErrorType.Unavailable);
}
