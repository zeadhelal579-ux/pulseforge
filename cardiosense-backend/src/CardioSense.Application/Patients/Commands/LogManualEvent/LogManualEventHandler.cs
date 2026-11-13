// =============================================================================
// File           : LogManualEventHandler.cs
// Layer          : Application / Patients/Commands/LogManualEvent
// Purpose        : Handles a manual event: creates the alert, saves it and pushes it to everyone watching the
//                  patient.
// Logic          : Doctor-only action. A Critical manual event also latches the patient's status until a
//                  doctor resolves the alert.
// Implementation : Internal sealed handler; the push is best-effort (IRealtimeNotifier implementations never
//                  throw on transport errors).
// =============================================================================
using CardioSense.Application.Common.Dtos;
using CardioSense.Application.Common.Errors;
using CardioSense.Application.Common.Interfaces;
using CardioSense.Application.Common.Mappings;
using CardioSense.Domain.Common;
using CardioSense.Domain.Entities;
using CardioSense.Domain.Enums;
using Mediator;

namespace CardioSense.Application.Patients.Commands.LogManualEvent;

/// <summary>Handles <see cref="LogManualEventCommand"/>.</summary>
internal sealed class LogManualEventHandler(
    IAppDbContext db, IRealtimeNotifier notifier, ICurrentUser currentUser, TimeProvider clock)
    : IRequestHandler<LogManualEventCommand, Result<AlertDto>>
{
    /// <summary>Creates and publishes the manual-event alert.</summary>
    public async ValueTask<Result<AlertDto>> Handle(LogManualEventCommand request, CancellationToken ct)
    {
        if (currentUser.Role != UserRole.Doctor)
        {
            return Result.Failure<AlertDto>(ApplicationErrors.Doctor.DoctorOnly);
        }

        var alert = Alert.Create(
            request.PatientId, AlertType.ManualEvent, request.Severity, request.Note, clock.GetUtcNow());
        db.Alerts.Add(alert);
        await db.SaveChangesAsync(ct);

        var dto = alert.ToDto();
        await notifier.NotifyAlertAsync(dto, ct);
        return Result.Success(dto);
    }
}
