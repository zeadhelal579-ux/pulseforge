// =============================================================================
// File           : GetWardStatsHandler.cs
// Layer          : Application / Doctors/Queries/GetWardStats
// Purpose        : Handles the ward statistics of one doctor.
// Logic          : '42 active patients' means patients assigned to this doctor. Status counts add up to the
//                  caseload (a patient with no assessment yet counts as stable); signal accuracy is the
//                  average confidence of each patient's latest electrode check.
// Implementation : Internal sealed handler; every number is derived from the caseload helpers, devices and
//                  electrode checks - nothing is hard-coded except the configurable capacity.
// =============================================================================
using CardioSense.Application.Common.Dtos;
using CardioSense.Application.Common.Errors;
using CardioSense.Application.Common.Interfaces;
using CardioSense.Application.Common.Options;
using CardioSense.Application.Doctors.Common;
using CardioSense.Domain.Common;
using CardioSense.Domain.Enums;
using Mediator;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace CardioSense.Application.Doctors.Queries.GetWardStats;

/// <summary>Handles <see cref="GetWardStatsQuery"/>.</summary>
internal sealed class GetWardStatsHandler(
    IAppDbContext db, ICurrentUser currentUser, IOptions<MonitoringOptions> options, TimeProvider clock)
    : IRequestHandler<GetWardStatsQuery, Result<WardStatsDto>>
{
    /// <summary>Computes the statistics of the doctor's caseload.</summary>
    public async ValueTask<Result<WardStatsDto>> Handle(GetWardStatsQuery request, CancellationToken ct)
    {
        if (currentUser.DoctorId is not { } doctorId)
        {
            return Result.Failure<WardStatsDto>(ApplicationErrors.Doctor.DoctorOnly);
        }

        var patientIds = await db.GetPatientIdsAsync(doctorId, ct);
        var assessments = await db.GetLatestAssessmentsAsync(patientIds, ct);
        var electrodeChecks = await db.GetLatestElectrodeVerificationsAsync(patientIds, ct);
        var devices = await db.Devices.AsNoTracking()
            .Where(d => d.PatientId != null && patientIds.Contains(d.PatientId.Value))
            .ToListAsync(ct);

        var now = clock.GetUtcNow();
        var activePatients = patientIds.Count;
        var criticalCount = assessments.Values.Count(a => a.StatusLevel == RiskStatusLevel.Critical);
        var cautionCount = assessments.Values.Count(a => a.StatusLevel == RiskStatusLevel.Caution);
        var connectedDevices = devices.Count(d => d.IsLive(now, options.Value.DeviceLiveWindow));

        var linkRate = devices.Count == 0 ? 0 : Math.Round(connectedDevices * 100.0 / devices.Count, 1);
        var signalAccuracy = electrodeChecks.Count == 0
            ? 0
            : Math.Round(electrodeChecks.Values.Average(v => v.ConfidenceScore) * 100, 1);

        return Result.Success(new WardStatsDto(
            activePatients,
            Math.Max(options.Value.DoctorPatientCapacity, activePatients),
            criticalCount,
            activePatients,
            connectedDevices,
            devices.Count,
            linkRate,
            signalAccuracy,
            activePatients - criticalCount - cautionCount,
            cautionCount));
    }
}
