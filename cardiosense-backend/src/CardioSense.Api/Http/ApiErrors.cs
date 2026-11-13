// =============================================================================
// File           : ApiErrors.cs
// Layer          : Api / Http
// Purpose        : The few errors that only the HTTP layer can produce (everything else comes from Application).
// Logic          : Same Error type as the rest of the solution so every failure leaves the API through the one
//                  ToProblem mapping and has the same JSON shape.
// Implementation : Static readonly Error instances.
// =============================================================================
using CardioSense.Domain.Common;

namespace CardioSense.Api.Http;

/// <summary>Errors raised by the HTTP layer itself.</summary>
internal static class ApiErrors
{
    /// <summary>A unique index rejected the write (two requests raced past the application-level check).</summary>
    public static readonly Error UniqueViolation = Error.Conflict(
        "Database.UniqueViolation", "A record with the same unique value already exists.");

    /// <summary>A cookie-based call came from a browser origin that is not on the allow-list.</summary>
    public static readonly Error OriginNotAllowed = Error.Forbidden(
        "Request.OriginNotAllowed", "The request origin is not allowed.");
}
