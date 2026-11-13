// =============================================================================
// File           : ProcessTelemetryTests.cs
// Layer          : Tests / Pipeline
// Purpose        : Verifies the core use case: a device reading in; stored data, AI assessment, alerts, pushes out.
// Logic          : Covers the rules a clinician depends on - unknown devices are not trusted, a reading is kept even
//                  when the AI is down, a critical status raises one alert (not one per reading) and stays critical
//                  until a doctor resolves it, recovery closes lower alerts, and a patient never receives the
//                  doctor-only AI numbers or flags.
// Implementation : Seeds a patient with a paired device, scripts the AI answer, sends readings through the pipeline,
//                  then inspects the database, the vitals store and the recorded pushes.
// =============================================================================
using CardioSense.Application.Common.Errors;
using CardioSense.Application.Common.Models;
using CardioSense.Application.Patients.Commands.ResolveAlert;
using CardioSense.Application.Telemetry.Commands.ProcessTelemetry;
using CardioSense.Application.Tests.Support;
using CardioSense.Domain.Common;
using CardioSense.Domain.Entities;
using CardioSense.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace CardioSense.Application.Tests.Pipeline;

/// <summary>Tests for processing a device reading.</summary>
public sealed class ProcessTelemetryTests
{
    /// <summary>Serial number of the test device.</summary>
    private const string Serial = "CS-TEST-0001";

    /// <summary>Builds a plausible reading stamped with the host's current time.</summary>
    private static ProcessTelemetryCommand Reading(ApplicationTestHost host, string serial = Serial) =>
        new(serial, host.Clock.GetUtcNow(), 74, 98, 152, 88, 396, 418, null, null);

    /// <summary>Seeds a patient whose device is paired and returns the patient.</summary>
    private static async Task<Patient> SeedMonitoredPatientAsync(ApplicationTestHost host)
    {
        var patient = Seed.NewPatient();
        await host.SeedAsync(patient, Seed.NewDevice(Serial, pairedTo: patient.Id));
        return patient;
    }

    /// <summary>Sends a reading and moves the clock on, so consecutive readings have distinct times.</summary>
    private static async Task<Result> SendReadingAsync(ApplicationTestHost host)
    {
        var result = await host.SendAsync(Reading(host));
        host.Clock.Advance(TimeSpan.FromMinutes(1));
        return result;
    }

    /// <summary>The most recently stored status.</summary>
    private static Task<RiskStatusLevel> LatestStatusAsync(ApplicationTestHost host) =>
        host.QueryAsync(db => db.RiskAssessments
            .OrderByDescending(risk => risk.Timestamp)
            .Select(risk => risk.StatusLevel)
            .FirstAsync());

    /// <summary>A serial number nobody registered is not trusted.</summary>
    [Fact]
    public async Task An_unknown_device_is_rejected()
    {
        await using var host = new ApplicationTestHost();

        var result = await host.SendAsync(Reading(host, "UNKNOWN"));

        Assert.True(result.IsFailure);
        Assert.Equal(ApplicationErrors.Telemetry.UnknownDevice.Code, result.Error.Code);
    }

    /// <summary>A device that is in inventory but not paired to a patient has nobody to attach readings to.</summary>
    [Fact]
    public async Task An_unpaired_device_is_rejected()
    {
        await using var host = new ApplicationTestHost();
        await host.SeedAsync(Seed.NewDevice(Serial));

        var result = await host.SendAsync(Reading(host));

        Assert.Equal(ApplicationErrors.Telemetry.UnknownDevice.Code, result.Error.Code);
    }

    /// <summary>A normal reading is stored, pushed to browsers and assessed; it raises no alert.</summary>
    [Fact]
    public async Task A_normal_reading_is_stored_pushed_and_assessed()
    {
        await using var host = new ApplicationTestHost();
        var patient = await SeedMonitoredPatientAsync(host);

        var result = await host.SendAsync(Reading(host));

        Assert.True(result.IsSuccess);
        var sample = Assert.Single(host.Vitals.Samples);
        Assert.Equal(74, sample.HeartRate);
        Assert.Equal(patient.Id, Assert.Single(host.Notifier.Vitals).PatientId);
        Assert.Equal(RiskStatusLevel.Stable, await LatestStatusAsync(host));
        Assert.Equal(0, await host.QueryAsync(db => db.Alerts.CountAsync()));
    }

    /// <summary>The AI receives the patient's age and the reading's features.</summary>
    [Fact]
    public async Task The_ai_is_asked_with_the_patients_age_and_features()
    {
        await using var host = new ApplicationTestHost();
        var patient = await SeedMonitoredPatientAsync(host);

        await host.SendAsync(Reading(host));

        var request = Assert.Single(host.RiskService.Requests);
        Assert.Equal(patient.Id, request.PatientId);
        Assert.Equal(58, request.Context.Age);
        Assert.Equal(152, request.Features.PrIntervalMs);
    }

    /// <summary>A critical status raises one alert whose text and pushes hide the doctor-only AI details.</summary>
    [Fact]
    public async Task A_critical_status_raises_one_alert_and_hides_clinical_details_from_the_patient()
    {
        await using var host = new ApplicationTestHost();
        await SeedMonitoredPatientAsync(host);
        host.RiskService.NextResult =
            StubRiskAssessmentService.Answer(RiskStatusLevel.Critical, "possible_st_elevation");

        await SendReadingAsync(host);
        await SendReadingAsync(host);

        var alert = await host.QueryAsync(db => db.Alerts.SingleAsync());
        Assert.Equal(AlertSeverity.Critical, alert.Severity);
        Assert.DoesNotContain("possible_st_elevation", alert.Message);
        Assert.Single(host.Notifier.Alerts);

        var broadcast = host.Notifier.RiskStatuses[0];
        Assert.Equal(0.9, broadcast.Clinical.ConfidenceScore);
        Assert.Contains("possible_st_elevation", broadcast.Clinical.Flags);
        Assert.Null(broadcast.PatientView.ConfidenceScore);
        Assert.Null(broadcast.PatientView.RiskScore);
        Assert.Empty(broadcast.PatientView.Flags);
    }

    /// <summary>When the AI is down the reading is still kept and no assessment is invented.</summary>
    [Fact]
    public async Task When_the_ai_is_unavailable_the_reading_is_still_saved()
    {
        await using var host = new ApplicationTestHost();
        await SeedMonitoredPatientAsync(host);
        host.RiskService.NextResult = Result.Failure<RiskAssessmentResult>(
            Error.Unavailable("Ai.Unavailable", "The AI service is down."));

        var result = await host.SendAsync(Reading(host));

        Assert.True(result.IsSuccess);
        Assert.Single(host.Vitals.Samples);
        Assert.Equal(1, await host.QueryAsync(db => db.EcgFeatureSets.CountAsync()));
        Assert.Equal(0, await host.QueryAsync(db => db.RiskAssessments.CountAsync()));
    }

    /// <summary>A critical status does not drop to Stable on its own, only after a doctor resolves the alert.</summary>
    [Fact]
    public async Task A_critical_status_stays_until_a_doctor_resolves_the_alert()
    {
        await using var host = new ApplicationTestHost();
        var patient = await SeedMonitoredPatientAsync(host);
        var doctor = Seed.NewDoctor();
        await host.SeedAsync(doctor, Seed.Assign(doctor, patient));

        host.RiskService.NextResult = StubRiskAssessmentService.Answer(RiskStatusLevel.Critical);
        await SendReadingAsync(host);

        host.RiskService.NextResult = StubRiskAssessmentService.Answer(RiskStatusLevel.Stable);
        await SendReadingAsync(host);
        Assert.Equal(RiskStatusLevel.Critical, await LatestStatusAsync(host));

        var alertId = await host.QueryAsync(db => db.Alerts.Select(alert => alert.Id).SingleAsync());
        host.CurrentUser.SignInAsDoctor(doctor.Id);
        var resolved = await host.SendAsync(new ResolveAlertCommand(patient.Id, alertId, "Reviewed by doctor."));
        Assert.True(resolved.IsSuccess);

        await SendReadingAsync(host);
        Assert.Equal(RiskStatusLevel.Stable, await LatestStatusAsync(host));
    }

    /// <summary>Caution after Stable raises a moderate alert, and recovery to Stable closes it automatically.</summary>
    [Fact]
    public async Task A_caution_alert_is_closed_automatically_when_the_status_recovers()
    {
        await using var host = new ApplicationTestHost();
        await SeedMonitoredPatientAsync(host);

        host.RiskService.NextResult = StubRiskAssessmentService.Answer(RiskStatusLevel.Stable);
        await SendReadingAsync(host);
        host.RiskService.NextResult = StubRiskAssessmentService.Answer(RiskStatusLevel.Caution);
        await SendReadingAsync(host);

        var raised = await host.QueryAsync(db => db.Alerts.SingleAsync());
        Assert.Equal(AlertSeverity.Moderate, raised.Severity);
        Assert.Null(raised.ResolvedAt);

        host.RiskService.NextResult = StubRiskAssessmentService.Answer(RiskStatusLevel.Stable);
        await SendReadingAsync(host);

        var closed = await host.QueryAsync(db => db.Alerts.SingleAsync());
        Assert.NotNull(closed.ResolvedAt);
    }

    /// <summary>A reading from the future is rejected instead of being stored with a wrong time.</summary>
    [Fact]
    public async Task A_reading_from_the_future_is_rejected()
    {
        await using var host = new ApplicationTestHost();
        await SeedMonitoredPatientAsync(host);
        var future = Reading(host) with { Timestamp = host.Clock.GetUtcNow().AddHours(2) };

        var result = await host.SendAsync(future);

        Assert.Equal(ErrorType.Validation, result.Error.Type);
        Assert.Empty(host.Vitals.Samples);
    }
}
