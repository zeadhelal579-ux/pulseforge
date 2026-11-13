// =============================================================================
// File           : GetPatientAiReportHandler.cs
// Layer          : Application / Patients/Queries/GetPatientAiReport
// Purpose        : Handles GetPatientAiReportQuery: computes the period's numbers, then asks the AI service
//                  for the narrative.
// Logic          : No data in the period means NotFound (the UI then shows its empty state) - a report is
//                  never invented. If the AI service is down the failure is passed through so the UI shows
//                  its empty state instead of fake text.
// Implementation : Internal sealed handler; the narrative text is displayed verbatim by the frontend, so it
//                  must come only from the AI service.
// =============================================================================
using CardioSense.Application.Common.Contracts;
using CardioSense.Application.Common.Dtos;
using CardioSense.Application.Common.Errors;
using CardioSense.Application.Common.Interfaces;
using CardioSense.Application.Common.Models;
using CardioSense.Domain.Common;
using CardioSense.Domain.Enums;
using CardioSense.Domain.Interfaces;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace CardioSense.Application.Patients.Queries.GetPatientAiReport;

/// <summary>Handles <see cref="GetPatientAiReportQuery"/>.</summary>
internal sealed class GetPatientAiReportHandler(
    IAppDbContext db, IVitalsTimeSeriesStore vitals, IRiskAssessmentService ai, TimeProvider clock,
    ICurrentUser currentUser)
    : IRequestHandler<GetPatientAiReportQuery, Result<AiReportDto>>
{
    /// <summary>Builds the report for the requested period.</summary>
    public async ValueTask<Result<AiReportDto>> Handle(GetPatientAiReportQuery request, CancellationToken ct)
    {
        if (!ContractLiterals.ReportPeriods.TryResolve(request.Period, out var window))
        {
            return Result.Failure<AiReportDto>(
                Error.Validation("Report.InvalidPeriod", "Period must be 'daily', 'weekly' or 'monthly'."));
        }

        var patient = await db.Patients.AsNoTracking().FirstOrDefaultAsync(p => p.Id == request.PatientId, ct);
        if (patient is null)
        {
            return Result.Failure<AiReportDto>(ApplicationErrors.Patient.NotFound);
        }

        var now = clock.GetUtcNow();
        var from = now - window;

        var averageHeartRate = await vitals.GetAverageHeartRateAsync(patient.Id, from, now, ct);
        var highestRiskScore = await db.RiskAssessments
            .Where(r => r.PatientId == patient.Id && r.Timestamp >= from)
            .MaxAsync(r => r.RiskScore, ct);
        var alertsLogged = await db.Alerts.CountAsync(a => a.PatientId == patient.Id && a.TriggeredAt >= from, ct);

        if (averageHeartRate is null && highestRiskScore is null && alertsLogged == 0)
        {
            return Result.Failure<AiReportDto>(ApplicationErrors.Report.NoData);
        }

        var diagnoses = await db.MedicalHistoryEntries.AsNoTracking()
            .Where(e => e.PatientId == patient.Id && e.EntryType != MedicalHistoryEntryType.Medication)
            .Select(e => e.Description)
            .ToListAsync(ct);

        var period = request.Period.ToLowerInvariant();
        var visibleRiskScore = currentUser.Role == UserRole.Doctor ? highestRiskScore : null;
        var context = new PatientContext(patient.GetAge(DateOnly.FromDateTime(now.UtcDateTime)), diagnoses);
        var narrative = await ai.GenerateReportNarrativeAsync(
            new ReportNarrativeRequest(period, context, new ReportStatistics(averageHeartRate, visibleRiskScore,
            alertsLogged)),
            ct);

        if (narrative.IsFailure)
        {
            return Result.Failure<AiReportDto>(narrative.Error);
        }

        return Result.Success(new AiReportDto(
            period,
            narrative.Value,
            Math.Round(averageHeartRate ?? 0, 1),
            visibleRiskScore,
            alertsLogged,
            now));
    }
}
