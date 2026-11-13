// =============================================================================
// File           : IngestionErrors.cs
// Layer          : Infrastructure / Ingestion
// Purpose        : Reasons an incoming MQTT message is rejected before it reaches the application.
// Logic          : The worker logs these codes and drops the message; nothing malformed is ever trusted or retried.
// Implementation : Internal static catalogue of Error values plus one factory for the missing-fields case.
// =============================================================================
using CardioSense.Domain.Common;

namespace CardioSense.Infrastructure.Ingestion;

/// <summary>Errors raised while parsing device messages.</summary>
internal static class IngestionErrors
{
    /// <summary>The topic is not devices/{serial}/telemetry or devices/{serial}/status.</summary>
    public static readonly Error UnsupportedTopic = Error.Validation(
        "Ingestion.UnsupportedTopic", "The topic is not a supported device topic.");

    /// <summary>The payload is not a valid JSON object.</summary>
    public static readonly Error InvalidJson = Error.Validation(
        "Ingestion.InvalidJson", "The payload is not valid JSON.");

    /// <summary>Builds the error for required fields that are missing or invalid.</summary>
    public static Error MissingFields(IReadOnlyCollection<string> fields) => Error.Validation(
        "Ingestion.MissingFields", $"Missing or invalid fields: {string.Join(", ", fields)}.");
}
