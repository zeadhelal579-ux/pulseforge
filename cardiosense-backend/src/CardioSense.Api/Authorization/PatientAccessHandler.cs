// =============================================================================
// File           : PatientAccessHandler.cs
// Layer          : Api / Authorization
// Purpose        : Thin ASP.NET Core authorization adapter over IPatientAccessChecker.
// Logic          : "Can this caller see this patient?" is decided in exactly one place - IPatientAccessChecker in
//                  Application (a patient sees themself, a doctor sees assigned patients). This handler only lets the
//                  HTTP layer ask the same question, so a wrong patient id in the URL is rejected before the request
//                  even reaches the mediator. The mediator pipeline asks again; the two checks cannot disagree.
// Implementation : AuthorizationHandler over a Guid resource (the patient id).
// =============================================================================
using CardioSense.Application.Common.Interfaces;
using Microsoft.AspNetCore.Authorization;

namespace CardioSense.Api.Authorization;

/// <summary>Succeeds when the current caller may access the patient.</summary>
internal sealed class PatientAccessHandler(IPatientAccessChecker accessChecker)
    : AuthorizationHandler<PatientAccessRequirement, Guid>
{
    /// <summary>Asks the shared access checker and marks the requirement as met when it says yes.</summary>
    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context, PatientAccessRequirement requirement, Guid patientId)
    {
        if (await accessChecker.CanAccessAsync(patientId))
        {
            context.Succeed(requirement);
        }
    }
}
