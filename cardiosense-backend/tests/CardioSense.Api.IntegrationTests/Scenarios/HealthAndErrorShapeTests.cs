// =============================================================================
// File           : HealthAndErrorShapeTests.cs
// Layer          : Tests / Scenarios
// Purpose        : Verifies the probes, the standard security headers and the shape of error responses.
// Logic          : A consistent error shape is part of the contract with the frontend: every failure is
//                  application/problem+json with a status, and domain failures also carry a stable "code" (and, for
//                  validation, a per-field "errors" object). Responses with patient data must never be cacheable.
// Implementation : Real HTTP calls against the in-memory API.
// =============================================================================
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using CardioSense.Api.IntegrationTests.Support;

namespace CardioSense.Api.IntegrationTests.Scenarios;

/// <summary>Tests for health probes and error responses.</summary>
[Collection(ApiCollection.Name)]
public sealed class HealthAndErrorShapeTests(ApiFactory factory)
{
    /// <summary>The liveness probe answers without touching the database.</summary>
    [Fact]
    public async Task The_liveness_probe_answers_ok()
    {
        var response = await factory.CreateApiClient().GetAsync("/health/live");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    /// <summary>The readiness probe answers once the database is reachable.</summary>
    [Fact]
    public async Task The_readiness_probe_answers_ok_when_the_database_is_up()
    {
        var response = await factory.CreateApiClient().GetAsync("/health/ready");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    /// <summary>A protected endpoint without a token is 401, uncacheable, with a problem body.</summary>
    [Fact]
    public async Task A_protected_endpoint_without_a_token_is_401_and_uncacheable()
    {
        var response = await factory.CreateApiClient().GetAsync("/api/patients/me");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        Assert.Contains("no-store", response.Headers.CacheControl?.ToString());
        Assert.Equal("nosniff", Assert.Single(response.Headers.GetValues("X-Content-Type-Options")));
        Assert.Contains("json", response.Content.Headers.ContentType?.MediaType);
    }

    /// <summary>Invalid input is 400 with the problem fields and a per-field errors object.</summary>
    [Fact]
    public async Task Invalid_input_is_400_with_field_errors()
    {
        var response = await factory.CreateApiClient().PostAsJsonAsync("/api/auth/signup/patient", new
        {
            fullName = "Ahmed Mostafa Kamal",
            phone = "01000000000",
            email = "ahmed@example.com",
            password = "Passw0rd-123",
            age = 58,
            gender = "male",
            nationalId = "123",
            diagnosis = "Prior MI",
            preferredLanguage = "en"
        });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        using var body = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        Assert.Equal(400, body.RootElement.GetProperty("status").GetInt32());
        Assert.True(body.RootElement.GetProperty("errors").TryGetProperty("nationalId", out _));
    }

    /// <summary>A missing resource is 404 with the stable machine-readable code.</summary>
    [Fact]
    public async Task An_unknown_device_is_404_with_a_code()
    {
        var patient = await Accounts.NewPatientAsync(factory);

        var response = await patient.Client.PostAsJsonAsync(
            "/api/devices/pair", new { serialNumber = "NO-SUCH-DEVICE" });

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        using var body = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        Assert.Equal("Device.NotFound", body.RootElement.GetProperty("code").GetString());
    }
}
