// =============================================================================
// File           : PatientProfileAssembler.cs
// Layer          : Application / Patients/Common
// Purpose        : Builds the full PatientProfileDto aggregate (profile + latest clinical snapshot + 24h
//                  timeline).
// Logic          : Used by GetPatientProfile and UpdatePatientProfile so the aggregate is assembled in
//                  exactly one place. Doctor-only risk details are included only when includeClinicalDetails
//                  is true (patients get the status only).
// Implementation : Internal scoped service; runs a handful of small AsNoTracking queries (latest row per
//                  table) and one in-memory grouping for the hourly timeline, whose hours are in the server's
//                  configured time zone (TimeProvider.LocalTimeZone).
// =============================================================================
using CardioSense.Application.Common.Contracts;
using CardioSense.Application.Common.Dtos;
using CardioSense.Application.Common.Errors;
using CardioSense.Application.Common.Interfaces;
using CardioSense.Application.Common.Mappings;
using CardioSense.Domain.Common;
using CardioSense.Domain.Entities;
using CardioSense.Domain.Enums;
using CardioSense.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace CardioSense.Application.Patients.Common;

/// <summary>Assembles patient profile aggregates.</summary>
internal sealed class PatientProfileAssembler(
    IAppDbContext db, IIdentityService identity, IVitalsTimeSeriesStore vitals, TimeProvider clock)
{
    /// <summary>How far back the hourly status timeline looks.</summary>
    private static readonly TimeSpan TimelineWindow = TimeSpan.FromHours(24);

    /// <summary>Builds the aggregate for a patient, or fails with NotFound.</summary>
    public async Task<Result<PatientProfileDto>> BuildAsync(Guid patientId, bool includeClinicalDetails,
        CancellationToken ct)
    {
        var patient = await db.Patients.AsNoTracking().FirstOrDefaultAsync(p => p.Id == patientId, ct);
        if (patient is null)
        {
            return Result.Failure<PatientProfileDto>(ApplicationErrors.Patient.NotFound);
        }

        var account = await identity.FindByIdAsync(patient.UserId, ct);
        if (account is null)
        {
            return Result.Failure<PatientProfileDto>(ApplicationErrors.Patient.NotFound);
        }

        var now = clock.GetUtcNow();
        var doctor = await FindPrimaryDoctorAsync(patientId, ct);
        var device = await db.Devices.AsNoTracking().FirstOrDefaultAsync(d => d.PatientId == patientId, ct);
        var history = await db.MedicalHistoryEntries.AsNoTracking()
            .Where(e => e.PatientId == patientId)
            .OrderBy(e => e.RecordedAt)
            .ToListAsync(ct);

        var latestVitals = await vitals.GetLatestAsync(patientId, ct);
        var latestEcg = await db.EcgFeatureSets.AsNoTracking()
            .Where(e => e.PatientId == patientId)
            .OrderByDescending(e => e.Timestamp)
            .FirstOrDefaultAsync(ct);
        var latestRisk = await db.RiskAssessments.AsNoTracking()
            .Where(r => r.PatientId == patientId)
            .OrderByDescending(r => r.Timestamp)
            .FirstOrDefaultAsync(ct);
        var latestElectrode = await db.ElectrodeVerifications.AsNoTracking()
            .Where(v => v.PatientId == patientId)
            .OrderByDescending(v => v.Timestamp)
            .FirstOrDefaultAsync(ct);
        var timeline = await BuildTimelineAsync(patientId, now, ct);

        // The primary diagnosis is the first one recorded; medications are listed together.
        var diagnosis = history.FirstOrDefault(e => e.EntryType == MedicalHistoryEntryType.Diagnosis)?.Description
            ?? string.Empty;
        var medications = history
            .Where(e => e.EntryType == MedicalHistoryEntryType.Medication)
            .Select(e => e.Description)
            .ToList();

        return Result.Success(new PatientProfileDto(
            patient.Id,
            patient.FullName,
            patient.GetAge(DateOnly.FromDateTime(now.UtcDateTime)),
            ContractLiterals.Genders.ToLiteral(patient.Gender),
            patient.NationalId,
            patient.Phone,
            account.Email,
            diagnosis,
            medications.Count > 0 ? string.Join(", ", medications) : null,
            doctor?.ToAssignedDoctorDto(),
            device?.ToDto(),
            account.PreferredLanguage,
            history.Select(e => e.ToDto()).ToList(),
            latestVitals?.ToDto(),
            latestEcg?.ToDto(),
            latestRisk?.ToDto(includeClinicalDetails),
            latestElectrode?.ToDto(),
            timeline));
    }

    /// <summary>Finds the primary doctor, or the earliest assigned doctor when none is marked primary.</summary>
    private Task<Doctor?> FindPrimaryDoctorAsync(Guid patientId, CancellationToken ct) =>
        (from assignment in db.DoctorPatientAssignments.AsNoTracking()
         join doctor in db.Doctors.AsNoTracking() on assignment.DoctorId equals doctor.Id
         where assignment.PatientId == patientId
         orderby assignment.IsPrimary descending, assignment.AssignedAt
         select doctor).FirstOrDefaultAsync(ct);

    /// <summary>
    /// Worst status per clock hour (server time zone) over the last 24 hours; hours without data are skipped.
    /// </summary>
    private async Task<IReadOnlyList<HourlyStatusBucketDto>> BuildTimelineAsync(
        Guid patientId, DateTimeOffset now, CancellationToken ct)
    {
        var since = now - TimelineWindow;
        var points = await db.RiskAssessments.AsNoTracking()
            .Where(r => r.PatientId == patientId && r.Timestamp >= since)
            .Select(r => new { r.Timestamp, r.StatusLevel })
            .ToListAsync(ct);

        var zone = clock.LocalTimeZone;
        var currentHour = HourStart(now, zone);

        return points
            .GroupBy(point => HourStart(point.Timestamp, zone))
            .OrderBy(group => group.Key)
            .Select(group => new HourlyStatusBucketDto(
                group.Key.Hour, group.Max(point => point.StatusLevel), group.Key == currentHour))
            .ToList();
    }

    /// <summary>Truncates an instant to the start of its clock hour in the given time zone.</summary>
    private static DateTime HourStart(DateTimeOffset instant, TimeZoneInfo zone)
    {
        var local = TimeZoneInfo.ConvertTime(instant, zone);
        return new DateTime(local.Year, local.Month, local.Day, local.Hour, 0, 0);
    }
}
