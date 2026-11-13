// =============================================================================
// File           : IEndpointGroup.cs
// Layer          : Api / Endpoints
// Purpose        : Contract for a self-registering group of endpoints (one class per area).
// Logic          : Program.cs never lists endpoints. Adding an area means adding one class that implements this
//                  interface; EndpointExtensions finds it by reflection and maps it, so Program.cs does not change.
// Implementation : One method that receives the route builder and maps its routes on it.
// =============================================================================
namespace CardioSense.Api.Endpoints;

/// <summary>A group of related endpoints that maps itself.</summary>
internal interface IEndpointGroup
{
    /// <summary>Maps this group's routes.</summary>
    void Map(IEndpointRouteBuilder app);
}
