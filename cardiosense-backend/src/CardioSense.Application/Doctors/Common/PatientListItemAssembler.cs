// =============================================================================
// File           : PatientListItemAssembler.cs
// Layer          : Application / Doctors/Common
// Purpose        : Builds the list rows (PatientListItemDto) for a set of patients.
// Logic          : Used by GetMyPatients and by AssignDoctor (which returns the newly added row). A patient
//                  with no assessment yet is shown as 'Awaiting Data' (never as healthy); 'live' means the
//                  device was heard from inside the configured window.
// Implementation : Internal scoped service; loads every needed table once for ALL ids (no per-patient
//                  queries) and combines in memory.
// =============================================================================
using CardioSense.Application.Common.Dtos;
using CardioSense.Application.Common.Interfaces;
using CardioSense.Application.Common.Mappings;
using CardioSense.Application.Common.Options;
using CardioSense.Domain.Enums;
using CardioSense.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace CardioSense.Application.Doctors.Common;

/// <summary>Assembles patient list rows.</summary>
internal sealed class PatientListItemAssembler(
    IAppDbContext db, IVitalsTimeSeriesStore vitals, IOptions<MonitoringOptions> options, TimeProvider clock)
{
    /// <summary>Period of the min/max heart rate shown on each row.</summary>
    private static readonly TimeSpan HeartRateRangeWindow = TimeSpan.FromHours(24);

    /// <summary>Builds one row per given patient id.</summary>
    public async Task<List<PatientListItemDto>> BuildAsync(IReadOnlyCollection<Guid> patientIds, CancellationToken ct)
    {
        if (patientIds.Count == 0)
        {
            return [];
        }

        var now = clock.GetUtcNow();
        var today = DateOnly.FromDateTime(now.UtcDateTime);
        var liveWindow = options.Value.DeviceLiveWindow;

        var patients = await db.Patients.AsNoTracking().Where(p => patientIds.Contains(p.Id)).ToListAsync(ct);

        // The primary diagnosis of a patient is the first Diagnosis entry recorded.
        var diagnosisRows = await db.MedicalHistoryEntries.AsNoTracking()
            .Where(e => patientIds.Contains(e.PatientId) && e.EntryType == MedicalHistoryEntryType.Diagnosis)
            .OrderBy(e => e.RecordedAt)
            .Select(e => new { e.PatientId, e.Description })
            .ToListAsync(ct);
        var firstDiagnosis = diagnosisRows
            .GroupBy(row => row.PatientId)
            .ToDictionary(group => group.Key, group => group.First().Description);

        var assessments = await db.GetLatestAssessmentsAsync(patientIds, ct);
        var latestVitals = await vitals.GetLatestForPatientsAsync(patientIds, ct);
        var ranges = await vitals.GetHeartRateRangesAsync(patientIds, now - HeartRateRangeWindow, ct);
        var devices = await db.Devices.AsNoTracking()
            .Where(d => d.PatientId != null && patientIds.Contains(d.PatientId.Value))
            .ToListAsync(ct);
        var deviceByPatient = devices
            .GroupBy(d => d.PatientId!.Value)
            .ToDictionary(group => group.Key, group => group.First());

        return patients.Select(patient =>
        {
            assessments.TryGetValue(patient.Id, out var assessment);
            latestVitals.TryGetValue(patient.Id, out var sample);
            ranges.TryGetValue(patient.Id, out var range);
            deviceByPatient.TryGetValue(patient.Id, out var device);

            var level = assessment?.StatusLevel ?? RiskStatusLevel.Stable;

            return new PatientListItemDto(
                patient.Id,
                patient.FullName,
                patient.GetAge(today),
                firstDiagnosis.GetValueOrDefault(patient.Id, string.Empty),
                patient.FileNumberText(),
                level,
                StatusLabels.For(level, assessment is not null),
                level == RiskStatusLevel.Stable ? null : StatusLabels.ClinicalAlertKind,
                device?.IsLive(now, liveWindow) ?? false,
                sample?.HeartRate ?? 0,
                new HeartRateRangeDto(range.Min, range.Max),
                sample?.Spo2 ?? 0,
                assessment?.RiskScore,
                sample?.Timestamp ?? assessment?.Timestamp ?? patient.CreatedAt);
        }).ToList();
    }
}
