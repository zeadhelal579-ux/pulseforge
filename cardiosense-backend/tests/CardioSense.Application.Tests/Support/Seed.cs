// =============================================================================
// File           : Seed.cs
// Layer          : Tests / Support
// Purpose        : Small factories for valid domain objects, so a test states only what matters to it.
// Logic          : Every entity has strict invariants (non-empty names, valid ranges); building them by hand in every
//                  test would bury the point of the test. Defaults are valid; a test overrides only what it checks.
// Implementation : Static helpers returning new, unsaved entities.
// =============================================================================
using CardioSense.Domain.Entities;
using CardioSense.Domain.Enums;

namespace CardioSense.Application.Tests.Support;

/// <summary>Builders for test data.</summary>
public static class Seed
{
    /// <summary>The calendar day the tests start on.</summary>
    public static readonly DateOnly Today = DateOnly.FromDateTime(ApplicationTestHost.Start.UtcDateTime);

    /// <summary>A valid patient aged 58.</summary>
    public static Patient NewPatient(string nationalId = "29001011234567", string fullName = "Ahmed Mostafa Kamal") =>
        Patient.Create(
            Guid.NewGuid(), fullName, Patient.DateOfBirthFromAge(58, Today), Gender.Male, nationalId,
            "01000000000", ApplicationTestHost.Start);

    /// <summary>A valid doctor.</summary>
    public static Doctor NewDoctor(string licenseNumber = "SYN-1001", string fullName = "Dr. Sara Hassan") =>
        Doctor.Create(
            Guid.NewGuid(), fullName, "Cardiology", licenseNumber, "Cairo University Hospital", "CCU",
            "01100000000", ApplicationTestHost.Start);

    /// <summary>A device in inventory, optionally already paired to a patient.</summary>
    public static Device NewDevice(string serialNumber = "CS-TEST-0001", Guid? pairedTo = null)
    {
        var device = Device.Register(serialNumber, ApplicationTestHost.Start);
        if (pairedTo is { } patientId)
        {
            device.PairTo(patientId, ApplicationTestHost.Start);
        }

        return device;
    }

    /// <summary>Assigns a doctor to a patient.</summary>
    public static DoctorPatientAssignment Assign(Doctor doctor, Patient patient, bool isPrimary = true) =>
        DoctorPatientAssignment.Create(doctor.Id, patient.Id, isPrimary, ApplicationTestHost.Start);
}
