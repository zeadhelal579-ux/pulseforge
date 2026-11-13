// =============================================================================
// File           : ResolveAlertHandler.cs
// Layer          : Application / Patients/Commands/ResolveAlert
// Purpose        : Handles alert resolution by a doctor.
// Logic          : The alert must belong to the patient in the request (so an id from another patient cannot
//                  be resolved through this route); the domain decides whether the resolution is allowed.
// Implementation : Internal sealed handler; only doctors reach Alert.Resolve with resolvedByDoctor = true.
// =============================================================================
using CardioSense.Application.Common.Dtos;
using CardioSense.Application.Common.Errors;
using CardioSense.Application.Common.Interfaces;
using CardioSense.Application.Common.Mappings;
using CardioSense.Domain.Common;
using CardioSense.Domain.Enums;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace CardioSense.Application.Patients.Commands.ResolveAlert;

/// <summary>Handles <see cref="ResolveAlertCommand"/>.</summary>
internal sealed class ResolveAlertHandler(IAppDbContext db, ICurrentUser currentUser, TimeProvider clock)
    : IRequestHandler<ResolveAlertCommand, Result<AlertDto>>
{
    /// <summary>Resolves the alert and returns it.</summary>
    public async ValueTask<Result<AlertDto>> Handle(ResolveAlertCommand request, CancellationToken ct)
    {
        if (currentUser.Role != UserRole.Doctor)
        {
            return Result.Failure<AlertDto>(ApplicationErrors.Doctor.DoctorOnly);
        }

        var alert = await db.Alerts.FirstOrDefaultAsync(
            a => a.Id == request.AlertId && a.PatientId == request.PatientId, ct);
        if (alert is null)
        {
            return Result.Failure<AlertDto>(ApplicationErrors.Alert.NotFound);
        }

        var resolved = alert.Resolve(request.ResolutionNote, resolvedByDoctor: true, clock.GetUtcNow());
        if (resolved.IsFailure)
        {
            return Result.Failure<AlertDto>(resolved.Error);
        }

        await db.SaveChangesAsync(ct);
        return Result.Success(alert.ToDto());
    }
}
