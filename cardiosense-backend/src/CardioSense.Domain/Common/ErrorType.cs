// =============================================================================
// File           : ErrorType.cs
// Layer          : Domain / Common
// Purpose        : Classifies a failed Result so outer layers can pick the right HTTP status.
// Logic          : Failure=422, Validation=400, Unauthorized=401, Forbidden=403, NotFound=404, Conflict=409,
//                  Unavailable=503 (the mapping itself is owned by the Api layer).
// Implementation : Plain enum with explicit values; no behaviour.
// =============================================================================
namespace CardioSense.Domain.Common;

/// <summary>Category of a failure carried by <see cref="Error"/>.</summary>
public enum ErrorType
{
    /// <summary>A business rule was violated.</summary>
    Failure = 0,

    /// <summary>The input data is invalid.</summary>
    Validation = 1,

    /// <summary>The caller is not authenticated or presented bad credentials.</summary>
    Unauthorized = 2,

    /// <summary>The caller is authenticated but not allowed to perform the action.</summary>
    Forbidden = 3,

    /// <summary>The requested resource does not exist.</summary>
    NotFound = 4,

    /// <summary>The action conflicts with the current state of a resource.</summary>
    Conflict = 5,

    /// <summary>A required downstream dependency (for example the AI service) is unavailable.</summary>
    Unavailable = 6
}
