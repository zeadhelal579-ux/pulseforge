// =============================================================================
// File           : AuthorizationPipelineTests.cs
// Layer          : Tests / Pipeline
// Purpose        : Verifies "who may see which patient" through the real mediator pipeline.
// Logic          : A patient sees only themself, a doctor sees only assigned patients, anyone else sees nothing. The
//                  rule is enforced once, by the AuthorizationBehavior, for every patient-scoped request - these tests
//                  use one such request (the alert list) to prove it for all of them.
// Implementation : Seeds patients, doctors and assignments, signs in as each, and sends GetPatientAlertsQuery.
// =============================================================================
using CardioSense.Application.Common.Errors;
using CardioSense.Application.Patients.Queries.GetPatientAlerts;
using CardioSense.Application.Tests.Support;
using CardioSense.Domain.Common;

namespace CardioSense.Application.Tests.Pipeline;

/// <summary>Tests for patient-level authorization.</summary>
public sealed class AuthorizationPipelineTests
{
    /// <summary>A doctor with no assignment to the patient is refused.</summary>
    [Fact]
    public async Task A_doctor_who_is_not_assigned_is_refused()
    {
        await using var host = new ApplicationTestHost();
        var patient = Seed.NewPatient();
        var doctor = Seed.NewDoctor();
        await host.SeedAsync(patient, doctor);
        host.CurrentUser.SignInAsDoctor(doctor.Id);

        var result = await host.SendAsync(new GetPatientAlertsQuery(patient.Id));

        Assert.True(result.IsFailure);
        Assert.Equal(ErrorType.Forbidden, result.Error.Type);
        Assert.Equal(ApplicationErrors.Patient.AccessDenied.Code, result.Error.Code);
    }

    /// <summary>An assigned doctor can read the patient's alerts.</summary>
    [Fact]
    public async Task An_assigned_doctor_is_allowed()
    {
        await using var host = new ApplicationTestHost();
        var patient = Seed.NewPatient();
        var doctor = Seed.NewDoctor();
        await host.SeedAsync(patient, doctor, Seed.Assign(doctor, patient));
        host.CurrentUser.SignInAsDoctor(doctor.Id);

        var result = await host.SendAsync(new GetPatientAlertsQuery(patient.Id));

        Assert.True(result.IsSuccess);
    }

    /// <summary>A patient can read their own alerts but not another patient's.</summary>
    [Fact]
    public async Task A_patient_sees_only_their_own_record()
    {
        await using var host = new ApplicationTestHost();
        var own = Seed.NewPatient("29001011234567");
        var other = Seed.NewPatient("29001011234568", "Fatma Hassan Ibrahim");
        await host.SeedAsync(own, other);
        host.CurrentUser.SignInAsPatient(own.Id);

        var ownResult = await host.SendAsync(new GetPatientAlertsQuery(own.Id));
        var otherResult = await host.SendAsync(new GetPatientAlertsQuery(other.Id));

        Assert.True(ownResult.IsSuccess);
        Assert.Equal(ErrorType.Forbidden, otherResult.Error.Type);
    }

    /// <summary>A caller who is not signed in is refused.</summary>
    [Fact]
    public async Task An_anonymous_caller_is_refused()
    {
        await using var host = new ApplicationTestHost();
        var patient = Seed.NewPatient();
        await host.SeedAsync(patient);

        var result = await host.SendAsync(new GetPatientAlertsQuery(patient.Id));

        Assert.Equal(ErrorType.Forbidden, result.Error.Type);
    }
}
