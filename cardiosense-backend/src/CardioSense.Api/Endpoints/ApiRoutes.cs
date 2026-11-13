// =============================================================================
// File           : ApiRoutes.cs
// Layer          : Api / Endpoints
// Purpose        : Route and policy names that more than one file needs.
// Logic          : The SignalR hub path is used by the endpoint that maps the hub and by the JWT setup that accepts a
//                  query-string token on it; the rate-limit policy name is used by the auth endpoints and by its
//                  registration. One constant each keeps them in step.
// Implementation : Constants only.
// =============================================================================
namespace CardioSense.Api.Endpoints;

/// <summary>Shared route and policy names.</summary>
internal static class ApiRoutes
{
    /// <summary>Path of the SignalR monitoring hub (the frontend's VITE_SIGNALR_HUB_URL).</summary>
    public const string MonitoringHub = "/hubs/monitoring";

    /// <summary>Name of the rate-limit policy applied to login and sign-up.</summary>
    public const string AuthRateLimit = "auth";
}
