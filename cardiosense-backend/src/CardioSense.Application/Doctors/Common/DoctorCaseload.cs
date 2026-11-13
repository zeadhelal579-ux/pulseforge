// =============================================================================
// File           : DoctorCaseload.cs
// Layer          : Application / Doctors/Common
// Purpose        : Shared queries about a doctor's caseload: assigned patient ids, newest assessment and
//                  newest electrode check per patient.
// Logic          : Used by the patient list and the ward stats so 'latest per patient' is computed the same
//                  way everywhere. A doctor's caseload is the patients assigned to THAT doctor - never every
//                  patient in the database.
// Implementation : Internal extension methods on IAppDbContext; 'latest per patient' is a join against each
//                  patient's max timestamp (translates to plain SQL).
// =============================================================================
using CardioSense.Application.Common.Interfaces;
using CardioSense.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace CardioSense.Application.Doctors.Common;

/// <summary>Caseload queries.</summary>
internal static class DoctorCaseload
{
    /// <summary>Ids of the patients assigned to the doctor.</summary>
    public static Task<List<Guid>> GetPatientIdsAsync(this IAppDbContext db, Guid doctorId, CancellationToken ct) =>
        db.DoctorPatientAssignments.AsNoTracking()
            .Where(a => a.DoctorId == doctorId)
            .Select(a => a.PatientId)
            .Distinct()
            .ToListAsync(ct);

    /// <summary>The newest risk assessment of each given patient (patients without one are absent).</summary>
    public static async Task<Dictionary<Guid, RiskAssessment>> GetLatestAssessmentsAsync(
        this IAppDbContext db, IReadOnlyCollection<Guid> patientIds, CancellationToken ct)
    {
        var newest = db.RiskAssessments.AsNoTracking()
            .Where(r => patientIds.Contains(r.PatientId))
            .GroupBy(r => r.PatientId)
            .Select(group => new { PatientId = group.Key, Timestamp = group.Max(r => r.Timestamp) });

        var rows = await (
            from assessment in db.RiskAssessments.AsNoTracking()
            join latest in newest
                on new { assessment.PatientId, assessment.Timestamp } equals new { latest.PatientId, latest.Timestamp }
            select assessment).ToListAsync(ct);

        // Two rows with the exact same timestamp would clash as keys, so keep the first per patient.
        return rows.GroupBy(a => a.PatientId).ToDictionary(group => group.Key, group => group.First());
    }

    /// <summary>The newest electrode verification of each given patient (patients without one are absent).</summary>
    public static async Task<Dictionary<Guid, ElectrodeVerification>> GetLatestElectrodeVerificationsAsync(
        this IAppDbContext db, IReadOnlyCollection<Guid> patientIds, CancellationToken ct)
    {
        var newest = db.ElectrodeVerifications.AsNoTracking()
            .Where(v => patientIds.Contains(v.PatientId))
            .GroupBy(v => v.PatientId)
            .Select(group => new { PatientId = group.Key, Timestamp = group.Max(v => v.Timestamp) });

        var rows = await (
            from verification in db.ElectrodeVerifications.AsNoTracking()
            join latest in newest
                on new { verification.PatientId, verification.Timestamp } equals new { latest.PatientId,
                latest.Timestamp }
            select verification).ToListAsync(ct);

        return rows.GroupBy(v => v.PatientId).ToDictionary(group => group.Key, group => group.First());
    }
}
