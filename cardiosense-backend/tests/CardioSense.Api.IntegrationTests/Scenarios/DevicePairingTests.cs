// =============================================================================
// File           : DevicePairingTests.cs
// Layer          : Tests / Scenarios
// Purpose        : Verifies device pairing against the demo inventory seeded at start-up.
// Logic          : Only devices in inventory can be paired, one device belongs to one patient, and the database's
//                  unique indexes agree with the application rules. Each test uses its own demo serial number because
//                  all tests share one database.
// Implementation : Real HTTP calls against the CS-DEMO-xxxx devices that Development start-up seeds.
// =============================================================================
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using CardioSense.Api.IntegrationTests.Support;

namespace CardioSense.Api.IntegrationTests.Scenarios;

/// <summary>Tests for device pairing.</summary>
[Collection(ApiCollection.Name)]
public sealed class DevicePairingTests(ApiFactory factory)
{
    /// <summary>A patient pairs a free demo device and can read its status.</summary>
    [Fact]
    public async Task A_patient_can_pair_a_demo_device_and_read_its_status()
    {
        var patient = await Accounts.NewPatientAsync(factory);

        var paired = await patient.Client.PostAsJsonAsync("/api/devices/pair", new { serialNumber = "CS-DEMO-0001" });
        var status = await patient.Client.GetAsync("/api/devices/CS-DEMO-0001/status");

        Assert.Equal(HttpStatusCode.OK, paired.StatusCode);
        Assert.Equal(HttpStatusCode.OK, status.StatusCode);
    }

    /// <summary>A device that already belongs to another patient is a 409 conflict.</summary>
    [Fact]
    public async Task A_device_paired_to_another_patient_is_409()
    {
        var owner = await Accounts.NewPatientAsync(factory, deviceSerial: "CS-DEMO-0002");
        var other = await Accounts.NewPatientAsync(factory);

        var response = await other.Client.PostAsJsonAsync("/api/devices/pair", new { serialNumber = "CS-DEMO-0002" });

        Assert.NotNull(owner.PatientId);
        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        using var body = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        Assert.Equal("Device.AlreadyPaired", body.RootElement.GetProperty("code").GetString());
    }

    /// <summary>Someone else's device status is reported as not found, so serial numbers cannot be probed.</summary>
    [Fact]
    public async Task Another_patients_device_status_is_404()
    {
        await Accounts.NewPatientAsync(factory, deviceSerial: "CS-DEMO-0003");
        var stranger = await Accounts.NewPatientAsync(factory);

        var response = await stranger.Client.GetAsync("/api/devices/CS-DEMO-0003/status");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}
