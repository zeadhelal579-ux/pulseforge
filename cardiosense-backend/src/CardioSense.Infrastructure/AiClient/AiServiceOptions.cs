// =============================================================================
// File           : AiServiceOptions.cs
// Layer          : Infrastructure / AiClient
// Purpose        : Settings of the connection to the Python AI service.
// Logic          : The service is internal-only (never exposed to the internet), so these values point at its private
//                  address (docker network / VPC). A short timeout keeps a slow model from stalling telemetry
//                  processing - the handler already keeps the stored data when the AI does not answer.
// Implementation : Bound from the "AiService" configuration section and validated at start-up.
// =============================================================================
using System.ComponentModel.DataAnnotations;

namespace CardioSense.Infrastructure.AiClient;

/// <summary>Settings of the AI service client.</summary>
public sealed class AiServiceOptions
{
    /// <summary>Configuration section name.</summary>
    public const string SectionName = "AiService";

    /// <summary>Base address of the AI service (for example http://ai:8000).</summary>
    [Required]
    [Url]
    public string BaseUrl { get; set; } = "http://localhost:8000";

    /// <summary>How long one request may take before it counts as unavailable.</summary>
    [Range(1, 120)]
    public int TimeoutSeconds { get; set; } = 10;
}
