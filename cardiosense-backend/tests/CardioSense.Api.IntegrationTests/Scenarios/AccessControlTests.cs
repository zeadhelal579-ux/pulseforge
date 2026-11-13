// =============================================================================
// File           : AccessControlTests.cs
// Layer          : Tests / Scenarios
// Purpose        : Verifies who may see and do what, through the real HTTP stack and database.
// Logic          : The rules that protect patient data: a doctor sees only assigned patients, patients cannot use
//                  doctor endpoints or clinical actions, and every access to a patient record leaves an audit row
//                  (including refused attempts). One bug here would mean one patient's data reaching the wrong person,
//                  so these are the most important integration tests.
// Implementation : Creates patients and doctors through sign-up, then calls the API as each of them.
// =============================================================================
using System.Net;
using System.Net.Http.Json;
using CardioSense.Api.IntegrationTests.Support;
using CardioSense.Domain.Entities;
using CardioSense.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace CardioSense.Api.IntegrationTests.Scenarios;

/// <summary>Tests for authorization and auditing.</summary>
[Collection(ApiCollection.Name)]
public sealed class AccessControlTests(ApiFactory factory)
{
    /// <summary>Loads the audit rows written for a patient record.</summary>
    private async Task<List<AuditLogEntry>> AuditEntriesOfAsync(Guid? patientId)
    {
        await using var scope = factory.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        return await db.AuditLogEntries.Where(entry => entry.EntityId == patientId).ToListAsync();
    }

    /// <summary>
    /// Re-reads until the condition holds. The audit row is written after the response is sent, so the test
    /// may look a few milliseconds too early.
    /// </summary>
    private static async Task<List<AuditLogEntry>> EventuallyAsync(
        Func<Task<List<AuditLogEntry>>> read, Func<List<AuditLogEntry>, bool> condition)
    {
        var entries = await read();
        for (var attempt = 0; attempt < 20 && !condition(entries); attempt++)
        {
            await Task.Delay(TimeSpan.FromMilliseconds(250));
            entries = await read();
        }

        return entries;
    }

    /// <summary>A patient reads their own profile.</summary>
    [Fact]
    public async Task A_patient_can_read_their_own_profile()
    {
        var patient = await Accounts.NewPatientAsync(factory);

        var response = await patient.Client.GetAsync("/api/patients/me");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    /// <summary>A doctor who has not been assigned the patient is refused, then allowed after assignment.</summary>
    [Fact]
    public async Task A_doctor_sees_a_patient_only_after_assignment()
    {
        var patient = await Accounts.NewPatientAsync(factory);
        var doctor = await Accounts.NewDoctorAsync(factory);
        var url = $"/api/patients/{patient.PatientId}";

        var before = await doctor.Client.GetAsync(url);
        var assign = await doctor.Client.PostAsJsonAsync(
            "/api/doctors/me/patients", new { identifier = patient.NationalId });
        var after = await doctor.Client.GetAsync(url);

        Assert.Equal(HttpStatusCode.Forbidden, before.StatusCode);
        Assert.Equal(HttpStatusCode.OK, assign.StatusCode);
        Assert.Equal(HttpStatusCode.OK, after.StatusCode);
    }

    /// <summary>The doctor's list contains the assigned patient and nobody else.</summary>
    [Fact]
    public async Task The_doctors_list_contains_only_assigned_patients()
    {
        var assigned = await Accounts.NewPatientAsync(factory);
        await Accounts.NewPatientAsync(factory);
        var doctor = await Accounts.NewDoctorAsync(factory);
        await doctor.Client.PostAsJsonAsync("/api/doctors/me/patients", new { identifier = assigned.NationalId });

        var response = await doctor.Client.GetAsync("/api/doctors/me/patients");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadAsStringAsync();
        Assert.Contains(assigned.PatientId.ToString(), body);
        Assert.Contains("\"totalCount\":1", body);
    }

    /// <summary>A patient cannot use doctor endpoints.</summary>
    [Fact]
    public async Task A_patient_cannot_use_doctor_endpoints()
    {
        var patient = await Accounts.NewPatientAsync(factory);

        var response = await patient.Client.GetAsync("/api/doctors/me");

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    /// <summary>A patient cannot perform clinical actions, even on their own record.</summary>
    [Fact]
    public async Task A_patient_cannot_log_a_manual_event()
    {
        var patient = await Accounts.NewPatientAsync(factory);

        var response = await patient.Client.PostAsJsonAsync(
            $"/api/patients/{patient.PatientId}/manual-events", new { note = "Chest pain", severity = "Critical" });

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    /// <summary>An assigned doctor logs a manual event, which the patient then sees in their alerts.</summary>
    [Fact]
    public async Task An_assigned_doctor_can_log_a_manual_event()
    {
        var patient = await Accounts.NewPatientAsync(factory);
        var doctor = await Accounts.NewDoctorAsync(factory);
        await doctor.Client.PostAsJsonAsync("/api/doctors/me/patients", new { identifier = patient.NationalId });

        var logged = await doctor.Client.PostAsJsonAsync(
            $"/api/patients/{patient.PatientId}/manual-events",
            new { note = "Reported dizziness", severity = "Moderate" });
        var alerts = await patient.Client.GetStringAsync("/api/patients/me/alerts");

        Assert.Equal(HttpStatusCode.Created, logged.StatusCode);
        Assert.Contains("Reported dizziness", alerts);
    }

    /// <summary>Reading a patient's record - allowed or refused - is written to the audit log.</summary>
    [Fact]
    public async Task Access_to_a_patient_record_is_audited()
    {
        var patient = await Accounts.NewPatientAsync(factory);
        var stranger = await Accounts.NewDoctorAsync(factory);

        await patient.Client.GetAsync("/api/patients/me");
        await stranger.Client.GetAsync($"/api/patients/{patient.PatientId}");

        var entries = await EventuallyAsync(
            async () => await AuditEntriesOfAsync(patient.PatientId), found => found.Count >= 2);

        Assert.Contains(entries, entry => entry.UserId == patient.UserId && entry.Action.EndsWith("-> 200"));
        Assert.Contains(entries, entry => entry.UserId == stranger.UserId && entry.Action.EndsWith("-> 403"));
    }
}
