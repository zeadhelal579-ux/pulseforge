// =============================================================================
// File           : CurrentUserHubFilter.cs
// Layer          : Api / Authentication
// Purpose        : Makes ICurrentUser work inside SignalR hub methods.
// Logic          : MonitoringHub.JoinPatientGroup asks IPatientAccessChecker, which asks ICurrentUser. Inside a hub
//                  call there is no guarantee that IHttpContextAccessor holds a context (it depends on the
//                  transport), but the connection always knows its authenticated user. Copying that user into the
//                  scoped CurrentUser before each invocation makes the access check independent of the transport.
// Implementation : A SignalR IHubFilter registered in AddApi; it only runs for hub method invocations (connect and
//                  disconnect use Context.User directly).
// =============================================================================
using Microsoft.AspNetCore.SignalR;

namespace CardioSense.Api.Authentication;

/// <summary>Hands the connection's user to the scoped CurrentUser for every hub invocation.</summary>
internal sealed class CurrentUserHubFilter : IHubFilter
{
    /// <summary>Sets the current user for this invocation, then runs the hub method.</summary>
    public ValueTask<object?> InvokeMethodAsync(
        HubInvocationContext invocationContext, Func<HubInvocationContext, ValueTask<object?>> next)
    {
        if (invocationContext.Context.User is { } principal)
        {
            invocationContext.ServiceProvider.GetRequiredService<CurrentUser>().UsePrincipal(principal);
        }

        return next(invocationContext);
    }
}
