// =============================================================================
// File           : IPatientScopedRequest.cs
// Layer          : Application / Common/Interfaces
// Purpose        : Marker for requests that act on one specific patient.
// Logic          : AuthorizationBehavior inspects this marker and refuses the request unless the caller may
//                  access that patient - so no endpoint can forget the check.
// Implementation : Single read-only property implemented by the command/query records.
// =============================================================================
namespace CardioSense.Application.Common.Interfaces;

/// <summary>A request scoped to one patient.</summary>
public interface IPatientScopedRequest
{
    /// <summary>The patient the request acts on.</summary>
    Guid PatientId { get; }
}
