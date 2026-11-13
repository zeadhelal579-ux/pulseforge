// =============================================================================
// File           : HealthEndpoints.cs
// Layer          : Api / Endpoints
// Purpose        : Liveness and readiness probes for containers and load balancers.
// Logic          : /health/live only proves the process answers (no checks run). /health/ready additionally runs the
//                  checks tagged "ready" - today the database connection - so traffic is sent only to an instance that
//                  can actually serve it. Both are anonymous and carry no patient data.
// Implementation : Standard ASP.NET Core health-check endpoints.
// =============================================================================
using Microsoft.AspNetCore.Diagnostics.HealthChecks;

namespace CardioSense.Api.Endpoints;

/// <summary>Health probe endpoints.</summary>
internal sealed class HealthEndpoints : IEndpointGroup
{
    /// <summary>Tag of the checks that decide readiness.</summary>
    public const string ReadyTag = "ready";

    /// <summary>Maps /health/live and /health/ready.</summary>
    public void Map(IEndpointRouteBuilder app)
    {
        app.MapHealthChecks("/health/live", new HealthCheckOptions { Predicate = _ => false });
        app.MapHealthChecks("/health/ready", new HealthCheckOptions
        {
            Predicate = check => check.Tags.Contains(ReadyTag)
        });
    }
}
