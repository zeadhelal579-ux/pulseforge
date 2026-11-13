// =============================================================================
// File           : PairDeviceTests.cs
// Layer          : Tests / Pipeline
// Purpose        : Verifies device pairing rules.
// Logic          : Only devices that exist in inventory can be paired (so the broker cannot create devices), one
//                  patient has one device, one device has one patient, and nothing is replaced silently because
//                  swapping a device could stop someone's monitoring without anyone noticing.
// Implementation : Signs in as a patient, seeds devices and sends PairDeviceCommand.
// =============================================================================
using CardioSense.Application.Common.Errors;
using CardioSense.Application.Patients.Commands.PairDevice;
using CardioSense.Application.Tests.Support;
using CardioSense.Domain.Common;
using Microsoft.EntityFrameworkCore;

namespace CardioSense.Application.Tests.Pipeline;

/// <summary>Tests for pairing a device.</summary>
public sealed class PairDeviceTests
{
    /// <summary>A patient pairs a free inventory device to themself.</summary>
    [Fact]
    public async Task A_patient_can_pair_a_free_device()
    {
        await using var host = new ApplicationTestHost();
        var patient = Seed.NewPatient();
        await host.SeedAsync(patient, Seed.NewDevice("CS-TEST-0001"));
        host.CurrentUser.SignInAsPatient(patient.Id);

        var result = await host.SendAsync(new PairDeviceCommand("CS-TEST-0001", null));

        Assert.True(result.IsSuccess);
        var device = await host.QueryAsync(db => db.Devices.SingleAsync());
        Assert.Equal(patient.Id, device.PatientId);
    }

    /// <summary>A serial number that is not in inventory is not found.</summary>
    [Fact]
    public async Task An_unknown_serial_is_not_found()
    {
        await using var host = new ApplicationTestHost();
        var patient = Seed.NewPatient();
        await host.SeedAsync(patient);
        host.CurrentUser.SignInAsPatient(patient.Id);

        var result = await host.SendAsync(new PairDeviceCommand("NOPE", null));

        Assert.Equal(ApplicationErrors.Device.NotFound.Code, result.Error.Code);
    }

    /// <summary>A device paired to someone else cannot be taken over.</summary>
    [Fact]
    public async Task A_device_paired_to_another_patient_is_a_conflict()
    {
        await using var host = new ApplicationTestHost();
        var owner = Seed.NewPatient("29001011234567");
        var other = Seed.NewPatient("29001011234568", "Fatma Hassan Ibrahim");
        await host.SeedAsync(owner, other, Seed.NewDevice("CS-TEST-0001", pairedTo: owner.Id));
        host.CurrentUser.SignInAsPatient(other.Id);

        var result = await host.SendAsync(new PairDeviceCommand("CS-TEST-0001", null));

        Assert.Equal(ErrorType.Conflict, result.Error.Type);
        Assert.Equal(DomainErrors.Device.AlreadyPaired.Code, result.Error.Code);
    }

    /// <summary>A patient who already has a device cannot silently switch to another.</summary>
    [Fact]
    public async Task A_patient_with_a_device_cannot_pair_a_second_one()
    {
        await using var host = new ApplicationTestHost();
        var patient = Seed.NewPatient();
        await host.SeedAsync(
            patient,
            Seed.NewDevice("CS-TEST-0001", pairedTo: patient.Id),
            Seed.NewDevice("CS-TEST-0002"));
        host.CurrentUser.SignInAsPatient(patient.Id);

        var result = await host.SendAsync(new PairDeviceCommand("CS-TEST-0002", null));

        Assert.Equal(ApplicationErrors.Patient.AlreadyHasDevice.Code, result.Error.Code);
    }

    /// <summary>A doctor has to say which patient the device is for.</summary>
    [Fact]
    public async Task A_doctor_must_name_the_patient()
    {
        await using var host = new ApplicationTestHost();
        var doctor = Seed.NewDoctor();
        await host.SeedAsync(doctor, Seed.NewDevice("CS-TEST-0001"));
        host.CurrentUser.SignInAsDoctor(doctor.Id);

        var result = await host.SendAsync(new PairDeviceCommand("CS-TEST-0001", null));

        Assert.Equal(ApplicationErrors.Device.PatientRequired.Code, result.Error.Code);
    }
}
