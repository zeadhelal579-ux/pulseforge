// =============================================================================
// File           : AuthorizationBehavior.cs
// Layer          : Application / Common/Behaviors
// Purpose        : Mediator pipeline behavior that enforces patient-level access for every patient-scoped
//                  request.
// Logic          : If the request implements IPatientScopedRequest and the caller may not access that
//                  patient, the pipeline returns a Forbidden failure before validation or the handler run -
//                  one enforcement point for every entry (REST today, anything else tomorrow).
// Implementation : Open generic behavior registered first in AddApplication; requests that are not
//                  patient-scoped pass straight through.
// =============================================================================
using CardioSense.Application.Common.Errors;
using CardioSense.Application.Common.Interfaces;
using Mediator;

namespace CardioSense.Application.Common.Behaviors;

/// <summary>Enforces resource-based authorization for patient-scoped requests.</summary>
public sealed class AuthorizationBehavior<TMessage, TResponse>(IPatientAccessChecker accessChecker)
    : IPipelineBehavior<TMessage, TResponse>
    where TMessage : IMessage
{
    /// <summary>Checks access to the targeted patient, then continues down the pipeline.</summary>
    public async ValueTask<TResponse> Handle(
        TMessage message, MessageHandlerDelegate<TMessage, TResponse> next, CancellationToken cancellationToken)
    {
        if (message is IPatientScopedRequest scoped
            && !await accessChecker.CanAccessAsync(scoped.PatientId, cancellationToken))
        {
            return ResultFailureFactory<TResponse>.Create(ApplicationErrors.Patient.AccessDenied);
        }

        return await next(message, cancellationToken);
    }
}
