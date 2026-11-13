// =============================================================================
// File           : FreezeWaveformHandler.cs
// Layer          : Application / Patients/Commands/FreezeWaveform
// Purpose        : Handles the freeze/unfreeze acknowledgement.
// Logic          : Raw waveform is never stored, so there is no database change: the handler only allows
//                  doctors and writes a structured log line (patient id + doctor id + state) for the clinical
//                  record. Patient access was already enforced by the pipeline.
// Implementation : Internal sealed handler; the frontend ignores the response body and only needs a success
//                  status.
// =============================================================================
using CardioSense.Application.Common.Errors;
using CardioSense.Application.Common.Interfaces;
using CardioSense.Domain.Common;
using CardioSense.Domain.Enums;
using Mediator;
using Microsoft.Extensions.Logging;

namespace CardioSense.Application.Patients.Commands.FreezeWaveform;

/// <summary>Handles <see cref="FreezeWaveformCommand"/>.</summary>
internal sealed class FreezeWaveformHandler(ICurrentUser currentUser, ILogger<FreezeWaveformHandler> logger)
    : IRequestHandler<FreezeWaveformCommand, Result>
{
    /// <summary>Acknowledges the freeze state change.</summary>
    public ValueTask<Result> Handle(FreezeWaveformCommand request, CancellationToken ct)
    {
        if (currentUser.Role != UserRole.Doctor)
        {
            return ValueTask.FromResult(Result.Failure(ApplicationErrors.Doctor.DoctorOnly));
        }

        logger.LogInformation(
            "Waveform of patient {PatientId} {State} by doctor {DoctorId}.",
            request.PatientId,
            request.Frozen ? "frozen" : "unfrozen",
            currentUser.DoctorId);

        return ValueTask.FromResult(Result.Success());
    }
}
