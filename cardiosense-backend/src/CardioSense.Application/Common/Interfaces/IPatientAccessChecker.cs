// =============================================================================
// File           : IPatientAccessChecker.cs
// Layer          : Application / Common/Interfaces
// Purpose        : The single place that decides 'may the current caller access this patient?'.
// Logic          : A patient may access only themself; a doctor only patients assigned to them. Used by
//                  AuthorizationBehavior, by use cases that resolve the patient themselves, and (later) by
//                  the Api authorization handler and the SignalR hub.
// Implementation : Interface here; the implementation (PatientAccessChecker) is internal to the Application
//                  layer.
// =============================================================================
namespace CardioSense.Application.Common.Interfaces;

/// <summary>Resource-based authorization for patient data.</summary>
public interface IPatientAccessChecker
{
    /// <summary>True when the current caller may access the given patient.</summary>
    Task<bool> CanAccessAsync(Guid patientId, CancellationToken ct = default);
}
