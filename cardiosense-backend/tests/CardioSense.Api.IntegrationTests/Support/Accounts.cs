// =============================================================================
// File           : Accounts.cs
// Layer          : Tests / Support
// Purpose        : Creates patients and doctors through the real sign-up endpoints and hands back a signed-in client.
// Logic          : All tests share one database, so every account gets unique e-mail, national id and license number
//                  from a counter. Going through the public endpoints (not the database) keeps the tests honest: if
//                  sign-up breaks, they fail.
// Implementation : Static helpers returning an Account (client with bearer token + the ids from the response).
// =============================================================================
using System.Globalization;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using CardioSense.Application.Auth.Common;

namespace CardioSense.Api.IntegrationTests.Support;

/// <summary>A signed-in test user.</summary>
public sealed record Account(
    HttpClient Client, string AccessToken, Guid UserId, Guid? PatientId, Guid? DoctorId, string NationalId,
    string Email);

/// <summary>Creates test users through the API.</summary>
public static class Accounts
{
    /// <summary>JSON settings that read enums by name, like the API writes them.</summary>
    public static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web)
    {
        Converters = { new JsonStringEnumConverter() }
    };

    /// <summary>The password every test account uses.</summary>
    public const string Password = "Passw0rd-123";

    /// <summary>Source of unique values.</summary>
    private static int _counter;

    /// <summary>Signs up a patient and returns the signed-in account.</summary>
    public static async Task<Account> NewPatientAsync(ApiFactory factory, string? deviceSerial = null)
    {
        var n = Interlocked.Increment(ref _counter);
        var nationalId = (29000000000000L + n).ToString(CultureInfo.InvariantCulture);
        var email = $"patient{n}@example.com";

        var client = factory.CreateApiClient();
        var response = await client.PostAsJsonAsync("/api/auth/signup/patient", new
        {
            fullName = $"Patient Number {n}",
            phone = "01000000000",
            email,
            password = Password,
            age = 58,
            gender = "male",
            nationalId,
            diagnosis = "Prior MI",
            medications = "Aspirin",
            deviceSerial,
            preferredLanguage = "en"
        });

        return await SignedInAsync(client, response, nationalId, email);
    }

    /// <summary>Signs up a doctor and returns the signed-in account.</summary>
    public static async Task<Account> NewDoctorAsync(ApiFactory factory)
    {
        var n = Interlocked.Increment(ref _counter);
        var email = $"doctor{n}@example.com";

        var client = factory.CreateApiClient();
        var response = await client.PostAsJsonAsync("/api/auth/signup/doctor", new
        {
            fullName = $"Doctor Number {n}",
            phone = "01100000000",
            email,
            password = Password,
            specialty = "Cardiology",
            licenseNumber = $"SYN-{n}",
            hospital = "Cairo University Hospital",
            department = "CCU",
            preferredLanguage = "en"
        });

        return await SignedInAsync(client, response, string.Empty, email);
    }

    /// <summary>Reads the sign-up response and attaches the access token to the client.</summary>
    private static async Task<Account> SignedInAsync(
        HttpClient client, HttpResponseMessage response, string nationalId, string email)
    {
        response.EnsureSuccessStatusCode();
        var body = await response.Content.ReadFromJsonAsync<AuthResponse>(Json);
        Assert.NotNull(body);

        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", body.AccessToken);
        return new Account(
            client, body.AccessToken, body.User.Id, body.User.PatientId, body.User.DoctorId, nationalId, email);
    }
}
