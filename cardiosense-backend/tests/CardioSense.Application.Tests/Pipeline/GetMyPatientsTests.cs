// =============================================================================
// File           : GetMyPatientsTests.cs
// Layer          : Tests / Pipeline
// Purpose        : Verifies the doctor's patient list: it contains assigned patients only.
// Logic          : "42 active patients" on the doctor dashboard must mean the patients assigned to THIS doctor, not
//                  everyone in the database. Getting this wrong would leak one patient's data to the wrong doctor,
//                  so it is tested directly, together with search and the doctor-only rule.
// Implementation : Seeds patients and assignments, signs in, sends GetMyPatientsQuery.
// =============================================================================
using CardioSense.Application.Common.Errors;
using CardioSense.Application.Doctors.Queries.GetMyPatients;
using CardioSense.Application.Tests.Support;

namespace CardioSense.Application.Tests.Pipeline;

/// <summary>Tests for the doctor's caseload.</summary>
public sealed class GetMyPatientsTests
{
    /// <summary>Only patients assigned to the signed-in doctor appear.</summary>
    [Fact]
    public async Task Only_assigned_patients_are_listed()
    {
        await using var host = new ApplicationTestHost();
        var doctor = Seed.NewDoctor();
        var assigned = Seed.NewPatient("29001011234567");
        var notAssigned = Seed.NewPatient("29001011234568", "Fatma Hassan Ibrahim");
        await host.SeedAsync(doctor, assigned, notAssigned, Seed.Assign(doctor, assigned));
        host.CurrentUser.SignInAsDoctor(doctor.Id);

        var result = await host.SendAsync(new GetMyPatientsQuery());

        Assert.True(result.IsSuccess);
        Assert.Equal(1, result.Value.TotalCount);
        Assert.Equal(assigned.Id, Assert.Single(result.Value.Items).Id);
    }

    /// <summary>Search matches part of the name, ignoring case.</summary>
    [Fact]
    public async Task Search_filters_by_name()
    {
        await using var host = new ApplicationTestHost();
        var doctor = Seed.NewDoctor();
        var ahmed = Seed.NewPatient("29001011234567", "Ahmed Mostafa Kamal");
        var fatma = Seed.NewPatient("29001011234568", "Fatma Hassan Ibrahim");
        await host.SeedAsync(doctor, ahmed, fatma, Seed.Assign(doctor, ahmed), Seed.Assign(doctor, fatma, false));
        host.CurrentUser.SignInAsDoctor(doctor.Id);

        var result = await host.SendAsync(new GetMyPatientsQuery(Search: "FAT"));

        Assert.Equal(fatma.Id, Assert.Single(result.Value.Items).Id);
    }

    /// <summary>A patient cannot list patients.</summary>
    [Fact]
    public async Task A_patient_cannot_list_patients()
    {
        await using var host = new ApplicationTestHost();
        var patient = Seed.NewPatient();
        await host.SeedAsync(patient);
        host.CurrentUser.SignInAsPatient(patient.Id);

        var result = await host.SendAsync(new GetMyPatientsQuery());

        Assert.Equal(ApplicationErrors.Doctor.DoctorOnly.Code, result.Error.Code);
    }
}
