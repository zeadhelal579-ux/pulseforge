// =============================================================================
// File           : MonitoringHubTests.cs
// Layer          : Tests / Scenarios
// Purpose        : Verifies the SignalR hub with real authenticated connections.
// Logic          : A doctor may subscribe to a patient's live stream only when assigned to that patient - the same
//                  rule as the REST API, enforced by the same access checker. The connection uses long polling on
//                  purpose: there every message is a separate HTTP request, which is exactly where relying on
//                  HttpContext inside a hub would fail; the CurrentUserHubFilter makes the check work anyway.
// Implementation : SignalR client over the test server's handler, with the access token from sign-up.
// =============================================================================
using System.Net.Http.Json;
using CardioSense.Api.IntegrationTests.Support;
using Microsoft.AspNetCore.Http.Connections;
using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.SignalR.Client;

namespace CardioSense.Api.IntegrationTests.Scenarios;

/// <summary>Tests for the monitoring hub.</summary>
[Collection(ApiCollection.Name)]
public sealed class MonitoringHubTests(ApiFactory factory)
{
    /// <summary>Opens an authenticated connection to the hub.</summary>
    private HubConnection Connect(string accessToken) => new HubConnectionBuilder()
        .WithUrl(new Uri(factory.Server.BaseAddress, "/hubs/monitoring"), options =>
        {
            options.HttpMessageHandlerFactory = _ => factory.Server.CreateHandler();
            options.Transports = HttpTransportType.LongPolling;
            options.AccessTokenProvider = () => Task.FromResult<string?>(accessToken);
        })
        .Build();

    /// <summary>An assigned doctor can subscribe to the patient's live stream.</summary>
    [Fact]
    public async Task An_assigned_doctor_can_join_the_patient_group()
    {
        var patient = await Accounts.NewPatientAsync(factory);
        var doctor = await Accounts.NewDoctorAsync(factory);
        await doctor.Client.PostAsJsonAsync("/api/doctors/me/patients", new { identifier = patient.NationalId });

        await using var connection = Connect(doctor.AccessToken);
        await connection.StartAsync();

        await connection.InvokeAsync("JoinPatientGroup", patient.PatientId);
    }

    /// <summary>A doctor who is not assigned is refused with a hub error.</summary>
    [Fact]
    public async Task An_unassigned_doctor_cannot_join_the_patient_group()
    {
        var patient = await Accounts.NewPatientAsync(factory);
        var stranger = await Accounts.NewDoctorAsync(factory);

        await using var connection = Connect(stranger.AccessToken);
        await connection.StartAsync();

        var error = await Assert.ThrowsAsync<HubException>(
            () => connection.InvokeAsync("JoinPatientGroup", patient.PatientId));
        Assert.Contains("not allowed", error.Message);
    }

    /// <summary>A connection without a valid token cannot even connect.</summary>
    [Fact]
    public async Task A_connection_without_a_valid_token_is_rejected()
    {
        await using var connection = Connect("not-a-token");

        await Assert.ThrowsAnyAsync<Exception>(() => connection.StartAsync());
    }
}
