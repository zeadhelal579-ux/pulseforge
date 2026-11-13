// =============================================================================
// File           : OpenApiEndpoints.cs
// Layer          : Api / Endpoints
// Purpose        : Publishes the machine-readable API description (OpenAPI) at /openapi/v1.json.
// Logic          : The document lists every route and body shape, which is the quickest way for the frontend team to
//                  check the contract. It is mapped in Development only: a production API has no reason to advertise
//                  its full surface.
// Implementation : The environment is injected, because endpoint groups are created by the DI container.
// =============================================================================
namespace CardioSense.Api.Endpoints;

/// <summary>Maps the OpenAPI document in Development.</summary>
internal sealed class OpenApiEndpoints(IHostEnvironment environment) : IEndpointGroup
{
    /// <summary>Maps /openapi/v1.json when running in Development.</summary>
    public void Map(IEndpointRouteBuilder app)
    {
        if (environment.IsDevelopment())
        {
            app.MapOpenApi();
        }
    }
}
