// =============================================================================
// File           : RegisterPatientTests.cs
// Layer          : Tests / Pipeline
// Purpose        : Verifies patient sign-up end to end: account, profile, history, optional links, and the session.
// Logic          : Sign-up must be all-or-nothing and safe by default: a duplicate national id is a conflict, a doctor
//                  name is linked only when it identifies exactly one doctor (never guess whose patient this is),
//                  and a device is paired only when it exists and is free.
// Implementation : Sends RegisterPatientCommand through the pipeline against fakes and inspects the database.
// =============================================================================
using CardioSense.Application.Auth.Commands.RegisterPatient;
using CardioSense.Application.Common.Errors;
using CardioSense.Application.Tests.Support;
using CardioSense.Domain.Common;
using CardioSense.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace CardioSense.Application.Tests.Pipeline;

/// <summary>Tests for patient registration.</summary>
public sealed class RegisterPatientTests
{
    /// <summary>Builds a valid sign-up form, overriding only what a test cares about.</summary>
    private static RegisterPatientCommand Form(
        string email = "ahmed@example.com", string nationalId = "29001011234567", string? doctorName = null,
        string? deviceSerial = null) =>
        new("Ahmed Mostafa Kamal", "01000000000", email, "Passw0rd-123", 58, "male", nationalId, "Prior MI",
            "Aspirin", doctorName, deviceSerial, "en");

    /// <summary>A valid form creates the account, the profile and the history, and signs the patient in.</summary>
    [Fact]
    public async Task A_valid_form_creates_the_patient_and_a_session()
    {
        await using var host = new ApplicationTestHost();

        var result = await host.SendAsync(Form());

        Assert.True(result.IsSuccess);
        Assert.Equal(UserRole.Patient, result.Value.User.Role);
        Assert.NotNull(result.Value.User.PatientId);
        Assert.Equal(1, await host.QueryAsync(db => db.Patients.CountAsync()));
        Assert.Equal(2, await host.QueryAsync(db => db.MedicalHistoryEntries.CountAsync()));

        var storedHash = await host.QueryAsync(db => db.RefreshTokens.Select(token => token.TokenHash).SingleAsync());
        Assert.Equal(host.Tokens.HashRefreshToken(result.Value.RawRefreshToken), storedHash);
    }

    /// <summary>The same national id cannot register twice.</summary>
    [Fact]
    public async Task A_duplicate_national_id_is_a_conflict()
    {
        await using var host = new ApplicationTestHost();
        await host.SendAsync(Form());

        var result = await host.SendAsync(Form(email: "other@example.com"));

        Assert.Equal(ErrorType.Conflict, result.Error.Type);
        Assert.Equal(ApplicationErrors.Patient.NationalIdTaken.Code, result.Error.Code);
        Assert.Equal(1, await host.QueryAsync(db => db.Patients.CountAsync()));
    }

    /// <summary>A doctor name that matches exactly one doctor links them as the primary doctor.</summary>
    [Fact]
    public async Task A_unique_doctor_name_links_the_primary_doctor()
    {
        await using var host = new ApplicationTestHost();
        var doctor = Seed.NewDoctor(fullName: "Dr. Sara Hassan");
        await host.SeedAsync(doctor);

        await host.SendAsync(Form(doctorName: "Dr. Sara Hassan"));

        var assignment = await host.QueryAsync(db => db.DoctorPatientAssignments.SingleAsync());
        Assert.Equal(doctor.Id, assignment.DoctorId);
        Assert.True(assignment.IsPrimary);
    }

    /// <summary>A doctor name shared by two doctors links nobody, but registration still succeeds.</summary>
    [Fact]
    public async Task An_ambiguous_doctor_name_links_nobody()
    {
        await using var host = new ApplicationTestHost();
        await host.SeedAsync(
            Seed.NewDoctor("SYN-1", "Dr. Sara Hassan"),
            Seed.NewDoctor("SYN-2", "Dr. Sara Hassan"));

        var result = await host.SendAsync(Form(doctorName: "Dr. Sara Hassan"));

        Assert.True(result.IsSuccess);
        Assert.Equal(0, await host.QueryAsync(db => db.DoctorPatientAssignments.CountAsync()));
    }

    /// <summary>A free device from inventory is paired to the new patient.</summary>
    [Fact]
    public async Task A_free_device_serial_is_paired_to_the_new_patient()
    {
        await using var host = new ApplicationTestHost();
        await host.SeedAsync(Seed.NewDevice("CS-TEST-0001"));

        var result = await host.SendAsync(Form(deviceSerial: "CS-TEST-0001"));

        var device = await host.QueryAsync(db => db.Devices.SingleAsync());
        Assert.Equal(result.Value.User.PatientId, device.PatientId);
    }
}
