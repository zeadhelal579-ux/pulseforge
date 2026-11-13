// =============================================================================
// File           : AiErrors.cs
// Layer          : Infrastructure / AiClient
// Purpose        : The two ways the AI service can fail from the backend's point of view.
// Logic          : Both are "Unavailable" (HTTP 503 at the edge): either the service could not be reached/answered in
//                  time, or it answered with data that cannot be trusted. Callers never invent a result in either case.
// Implementation : Internal static catalogue of readonly Error values.
// =============================================================================
using CardioSense.Domain.Common;

namespace CardioSense.Infrastructure.AiClient;

/// <summary>Errors raised by the AI service client.</summary>
internal static class AiErrors
{
    /// <summary>The service was unreachable, too slow or answered with an error status.</summary>
    public static readonly Error Unavailable = Error.Unavailable(
        "Ai.Unavailable", "The AI service is currently unavailable.");

    /// <summary>The service answered, but the answer is incomplete or malformed.</summary>
    public static readonly Error InvalidAnswer = Error.Unavailable(
        "Ai.InvalidAnswer", "The AI service answered with data that cannot be used.");
}
