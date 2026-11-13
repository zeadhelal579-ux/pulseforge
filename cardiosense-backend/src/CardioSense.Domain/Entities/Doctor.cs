// =============================================================================
// File           : Doctor.cs
// Layer          : Domain / Entities
// Purpose        : Professional profile of a treating doctor.
// Logic          : License number and account link are fixed after creation; other professional fields can be
//                  edited.
// Implementation : Sealed entity with private setters and a static factory.
// =============================================================================
using CardioSense.Domain.Common;

namespace CardioSense.Domain.Entities;

/// <summary>A treating doctor.</summary>
public sealed class Doctor : Entity
{
    /// <summary>Maximum length of the full name.</summary>
    public const int NameMaxLength = 120;

    /// <summary>Maximum length of the specialty.</summary>
    public const int SpecialtyMaxLength = 120;

    /// <summary>Maximum length of the license number.</summary>
    public const int LicenseNumberMaxLength = 60;

    /// <summary>Maximum length of the hospital name.</summary>
    public const int HospitalMaxLength = 160;

    /// <summary>Maximum length of the department name.</summary>
    public const int DepartmentMaxLength = 120;

    /// <summary>Maximum length of the phone number.</summary>
    public const int PhoneMaxLength = 20;

    /// <summary>Used by EF Core.</summary>
    private Doctor()
    {
    }

    /// <summary>Builds a doctor after guarding every required value.</summary>
    private Doctor(
        Guid userId, string fullName, string specialty, string licenseNumber, string hospital,
        string? department, string phone, DateTimeOffset now)
        : base(now)
    {
        UserId = RequireId(userId, nameof(userId));
        FullName = RequireText(fullName, nameof(fullName), NameMaxLength);
        Specialty = RequireText(specialty, nameof(specialty), SpecialtyMaxLength);
        LicenseNumber = RequireText(licenseNumber, nameof(licenseNumber), LicenseNumberMaxLength);
        Hospital = RequireText(hospital, nameof(hospital), HospitalMaxLength);
        Department = RequireOptionalText(department, nameof(department), DepartmentMaxLength);
        Phone = RequireText(phone, nameof(phone), PhoneMaxLength);
    }

    /// <summary>The login account (Users table) that owns this profile.</summary>
    public Guid UserId { get; private set; }

    /// <summary>Full name.</summary>
    public string FullName { get; private set; } = null!;

    /// <summary>Medical specialty.</summary>
    public string Specialty { get; private set; } = null!;

    /// <summary>Syndicate / license number.</summary>
    public string LicenseNumber { get; private set; } = null!;

    /// <summary>Hospital the doctor works at.</summary>
    public string Hospital { get; private set; } = null!;

    /// <summary>Department (optional).</summary>
    public string? Department { get; private set; }

    /// <summary>Contact phone (used by the patient's call button).</summary>
    public string Phone { get; private set; } = null!;

    /// <summary>Creates a new doctor profile.</summary>
    public static Doctor Create(
        Guid userId, string fullName, string specialty, string licenseNumber, string hospital,
        string? department, string phone, DateTimeOffset now) =>
        new(userId, fullName, specialty, licenseNumber, hospital, department, phone, now);

    /// <summary>Updates the editable fields; null means "leave unchanged".</summary>
    public void UpdateProfile(string? fullName, string? phone, string? specialty, string? hospital, string? department)
    {
        if (fullName is not null)
        {
            FullName = RequireText(fullName, nameof(fullName), NameMaxLength);
        }

        if (phone is not null)
        {
            Phone = RequireText(phone, nameof(phone), PhoneMaxLength);
        }

        if (specialty is not null)
        {
            Specialty = RequireText(specialty, nameof(specialty), SpecialtyMaxLength);
        }

        if (hospital is not null)
        {
            Hospital = RequireText(hospital, nameof(hospital), HospitalMaxLength);
        }

        if (department is not null)
        {
            Department = RequireOptionalText(department, nameof(department), DepartmentMaxLength);
        }
    }
}
