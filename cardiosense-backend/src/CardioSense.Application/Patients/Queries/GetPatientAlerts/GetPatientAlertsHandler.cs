// =============================================================================
// File           : GetPatientAlertsHandler.cs
// Layer          : Application / Patients/Queries/GetPatientAlerts
// Purpose        : Handles GetPatientAlertsQuery.
// Logic          : Returns the newest alerts first and caps the list so an old, noisy patient file cannot
//                  return an unbounded payload.
// Implementation : Internal sealed handler with a single AsNoTracking query.
// =============================================================================
using CardioSense.Application.Common.Dtos;
using CardioSense.Application.Common.Interfaces;
using CardioSense.Application.Common.Mappings;
using CardioSense.Domain.Common;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace CardioSense.Application.Patients.Queries.GetPatientAlerts;

/// <summary>Handles <see cref="GetPatientAlertsQuery"/>.</summary>
internal sealed class GetPatientAlertsHandler(IAppDbContext db)
    : IRequestHandler<GetPatientAlertsQuery, Result<IReadOnlyList<AlertDto>>>
{
    /// <summary>Largest number of alerts returned in one response.</summary>
    private const int MaxAlerts = 100;

    /// <summary>Loads the newest alerts of the patient.</summary>
    public async ValueTask<Result<IReadOnlyList<AlertDto>>> Handle(GetPatientAlertsQuery request, CancellationToken ct)
    {
        var alerts = await db.Alerts.AsNoTracking()
            .Where(a => a.PatientId == request.PatientId)
            .OrderByDescending(a => a.TriggeredAt)
            .Take(MaxAlerts)
            .ToListAsync(ct);

        IReadOnlyList<AlertDto> timeline = alerts.Select(alert => alert.ToDto()).ToList();
        return Result.Success(timeline);
    }
}
