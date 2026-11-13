// =============================================================================
// File           : PatientAccessFilter.cs
// Layer          : Api / Authorization
// Purpose        : Endpoint filter that applies PatientAccessRequirement to every route containing {id} (a patient).
// Logic          : Declared once on the /api/patients/{id} route group, so no individual endpoint can forget the
//                  check. A caller who may not see the patient gets 403, the same answer the mediator pipeline gives.
// Implementation : Reads the id from the route, asks IAuthorizationService with the patient id as the resource.
// =============================================================================
using CardioSense.Api.Http;
using CardioSense.Application.Common.Errors;
using Microsoft.AspNetCore.Authorization;

namespace CardioSense.Api.Authorization;

/// <summary>Rejects requests for patients the caller may not access.</summary>
internal sealed class PatientAccessFilter(IAuthorizationService authorization) : IEndpointFilter
{
    /// <summary>Name of the route value that holds the patient id.</summary>
    public const string PatientIdRouteKey = "id";

    /// <summary>Checks access, then continues to the endpoint only when it is granted.</summary>
    public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {
        var http = context.HttpContext;

        if (!Guid.TryParse(http.Request.RouteValues[PatientIdRouteKey]?.ToString(), out var patientId))
        {
            return ApplicationErrors.Patient.NotFound.ToProblem();
        }

        var outcome = await authorization.AuthorizeAsync(http.User, patientId, PatientAccessRequirement.Instance);

        return outcome.Succeeded ? await next(context) : ApplicationErrors.Patient.AccessDenied.ToProblem();
    }
}
