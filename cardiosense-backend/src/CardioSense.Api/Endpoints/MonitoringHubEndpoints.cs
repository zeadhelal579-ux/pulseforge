// =============================================================================
// File           : MonitoringHubEndpoints.cs
// Layer          : Api / Endpoints
// Purpose        : Maps the SignalR monitoring hub (live ECG, vitals, risk status, alerts) to /hubs/monitoring.
// Logic          : The hub class lives in Infrastructure (the realtime notifier needs its type, and Infrastructure
//                  must not depend on Api); the Api only decides its URL. Authentication is required by the hub itself.
// Implementation : One MapHub call, discovered like every other endpoint group.
// =============================================================================
using CardioSense.Infrastructure.Realtime;

namespace CardioSense.Api.Endpoints;

/// <summary>Maps the real-time hub.</summary>
internal sealed class MonitoringHubEndpoints : IEndpointGroup
{
    /// <summary>Maps the hub path.</summary>
    public void Map(IEndpointRouteBuilder app) => app.MapHub<MonitoringHub>(ApiRoutes.MonitoringHub);
}
