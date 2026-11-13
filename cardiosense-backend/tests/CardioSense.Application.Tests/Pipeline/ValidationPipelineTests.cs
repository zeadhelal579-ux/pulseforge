// =============================================================================
// File           : ValidationPipelineTests.cs
// Layer          : Tests / Pipeline
// Purpose        : Verifies that invalid input is rejected before any handler runs, with errors per field.
// Logic          : The frontend shows each message under the matching form control, so the keys must be the camelCase
//                  property names the form uses (nationalId, not NationalId).
// Implementation : Sends invalid commands through the pipeline and inspects the Validation error.
// =============================================================================
using CardioSense.Application.Auth.Commands.RegisterPatient;
using CardioSense.Application.Patients.Commands.LogManualEvent;
using CardioSense.Application.Tests.Support;
using CardioSense.Domain.Common;
using CardioSense.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace CardioSense.Application.Tests.Pipeline;

/// <summary>Tests for request validation.</summary>
public sealed class ValidationPipelineTests
{
    /// <summary>An empty clinical note is reported against the "note" field.</summary>
    [Fact]
    public async Task An_empty_manual_event_note_is_reported_on_the_note_field()
    {
        await using var host = new ApplicationTestHost();
        var patient = Seed.NewPatient();
        var doctor = Seed.NewDoctor();
        await host.SeedAsync(patient, doctor, Seed.Assign(doctor, patient));
        host.CurrentUser.SignInAsDoctor(doctor.Id);

        var result = await host.SendAsync(new LogManualEventCommand(patient.Id, string.Empty, AlertSeverity.Info));

        Assert.Equal(ErrorType.Validation, result.Error.Type);
        var fields = result.Error.ValidationErrors;
        Assert.NotNull(fields);
        Assert.Contains("note", fields.Keys);
    }

    /// <summary>A national id that is not 14 digits is reported against the "nationalId" field.</summary>
    [Fact]
    public async Task A_short_national_id_is_reported_on_the_nationalId_field()
    {
        await using var host = new ApplicationTestHost();
        var command = new RegisterPatientCommand(
            "Ahmed Mostafa Kamal", "01000000000", "ahmed@example.com", "Passw0rd-123", 58, "male", "123",
            "Prior MI", null, null, null, "en");

        var result = await host.SendAsync(command);

        Assert.Equal(ErrorType.Validation, result.Error.Type);
        var fields = result.Error.ValidationErrors;
        Assert.NotNull(fields);
        Assert.Contains("nationalId", fields.Keys);
    }

    /// <summary>A rejected registration creates nothing.</summary>
    [Fact]
    public async Task A_rejected_registration_creates_no_patient()
    {
        await using var host = new ApplicationTestHost();
        var command = new RegisterPatientCommand(
            "Ahmed Mostafa Kamal", "01000000000", "not-an-email", "Passw0rd-123", 58, "male", "29001011234567",
            "Prior MI", null, null, null, "en");

        await host.SendAsync(command);

        var count = await host.QueryAsync(db => db.Patients.CountAsync());
        Assert.Equal(0, count);
    }
}
