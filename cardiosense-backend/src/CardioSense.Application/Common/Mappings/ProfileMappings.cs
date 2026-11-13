// =============================================================================
// File           : ProfileMappings.cs
// Layer          : Application / Common/Mappings
// Purpose        : Maps profile-related entities: history entry, doctor (as assigned doctor and as profile)
//                  and patient file number.
// Logic          : The doctor profile needs the account's e-mail/language, so that mapping receives the
//                  UserAccount.
// Implementation : Static extension methods; pure functions with no I/O.
// =============================================================================
using CardioSense.Application.Common.Dtos;
using CardioSense.Application.Common.Models;
using CardioSense.Domain.Entities;

namespace CardioSense.Application.Common.Mappings;

/// <summary>Profile mappings.</summary>
public static class ProfileMappings
{
    /// <summary>Maps a medical history entry.</summary>
    public static MedicalHistoryEntryDto ToDto(this MedicalHistoryEntry entry) => new(
        entry.Id, entry.EntryType, entry.Description, entry.RecordedAt);

    /// <summary>Maps a doctor to the small record shown on the patient's screen.</summary>
    public static AssignedDoctorDto ToAssignedDoctorDto(this Doctor doctor) => new(
        doctor.Id, doctor.FullName, doctor.Specialty, doctor.Phone);

    /// <summary>Maps a doctor plus the account data to the full doctor profile.</summary>
    public static DoctorProfileDto ToProfileDto(this Doctor doctor, UserAccount account) => new(
        doctor.Id,
        doctor.FullName,
        doctor.Specialty,
        doctor.LicenseNumber,
        doctor.Hospital,
        doctor.Department,
        doctor.Phone,
        account.Email,
        account.PreferredLanguage);

    /// <summary>Formats the patient's file number the way the dashboards show it (CS-0042).</summary>
    public static string FileNumberText(this Patient patient) => $"CS-{patient.FileNumber:D4}";
}
