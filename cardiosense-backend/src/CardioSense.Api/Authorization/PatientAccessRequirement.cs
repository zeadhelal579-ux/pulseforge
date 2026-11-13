// =============================================================================
// File           : PatientAccessRequirement.cs
// Layer          : Api / Authorization
// Purpose        : Marker requirement: "the caller may see this particular patient".
// Logic          : Carries no data; the patient id is the resource being authorized, and PatientAccessHandler decides.
// Implementation : Singleton instance, because the requirement has no state.
// =============================================================================
using Microsoft.AspNetCore.Authorization;

namespace CardioSense.Api.Authorization;

/// <summary>Requirement that the caller may access a given patient.</summary>
internal sealed class PatientAccessRequirement : IAuthorizationRequirement
{
    /// <summary>The only instance.</summary>
    public static readonly PatientAccessRequirement Instance = new();
}
