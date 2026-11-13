// =============================================================================
// File           : EndpointExtensions.cs
// Layer          : Api / Endpoints
// Purpose        : Finds every IEndpointGroup in the assembly and maps it.
// Logic          : Registration and mapping are two steps because mapping needs the finished service provider. Both are
//                  driven by reflection over this assembly, which is what keeps Program.cs unchanged when a new
//                  endpoint group is added.
// Implementation : Concrete classes implementing IEndpointGroup are registered as enumerable services, then resolved
//                  and mapped once at start-up.
// =============================================================================
using System.Reflection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace CardioSense.Api.Endpoints;

/// <summary>Registration and mapping of endpoint groups.</summary>
internal static class EndpointExtensions
{
    /// <summary>Registers every endpoint group found in the given assembly.</summary>
    public static IServiceCollection AddEndpointGroups(this IServiceCollection services, Assembly assembly)
    {
        var groups = assembly.DefinedTypes
            .Where(type => type is { IsAbstract: false, IsInterface: false }
                           && type.IsAssignableTo(typeof(IEndpointGroup)))
            .Select(type => ServiceDescriptor.Transient(typeof(IEndpointGroup), type));

        services.TryAddEnumerable(groups);
        return services;
    }

    /// <summary>Maps all registered endpoint groups.</summary>
    public static IEndpointRouteBuilder MapEndpointGroups(this IEndpointRouteBuilder app)
    {
        foreach (var group in app.ServiceProvider.GetServices<IEndpointGroup>())
        {
            group.Map(app);
        }

        return app;
    }
}
