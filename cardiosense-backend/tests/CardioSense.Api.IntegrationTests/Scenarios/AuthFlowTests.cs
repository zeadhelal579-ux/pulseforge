// =============================================================================
// File           : AuthFlowTests.cs
// Layer          : Tests / Scenarios
// Purpose        : Verifies sign-up, refresh, logout and the refresh cookie end to end.
// Logic          : The refresh token must live only in an httpOnly, Secure, path-limited cookie and never in a JSON
//                  body; refresh must work from the cookie alone; logout must kill the session; a cookie-driven call
//                  from a foreign website must be refused (CSRF); duplicates must be 409, not 500.
// Implementation : Real HTTP calls; the client keeps cookies like a browser would (HTTPS base address).
// =============================================================================
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using CardioSense.Api.IntegrationTests.Support;

namespace CardioSense.Api.IntegrationTests.Scenarios;

/// <summary>Tests for the authentication flow.</summary>
[Collection(ApiCollection.Name)]
public sealed class AuthFlowTests(ApiFactory factory)
{
    /// <summary>Name of the refresh cookie.</summary>
    private const string CookieName = "cardiosense_refresh";

    /// <summary>Reads the single Set-Cookie header of a response.</summary>
    private static string SetCookieOf(HttpResponseMessage response) =>
        Assert.Single(response.Headers.GetValues("Set-Cookie"));

    /// <summary>Sign-up returns a session: a token in the body, the refresh token only in a hardened cookie.</summary>
    [Fact]
    public async Task Sign_up_sets_a_hardened_refresh_cookie_and_keeps_it_out_of_the_body()
    {
        var client = factory.CreateApiClient();

        var response = await client.PostAsJsonAsync("/api/auth/signup/patient", new
        {
            fullName = "Cookie Check Patient",
            phone = "01000000000",
            email = "cookie-check@example.com",
            password = "Passw0rd-123",
            age = 40,
            gender = "female",
            nationalId = "29100000000001",
            diagnosis = "Hypertension",
            preferredLanguage = "ar"
        });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var cookie = SetCookieOf(response);
        Assert.StartsWith(CookieName + "=", cookie);
        Assert.Contains("httponly", cookie, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("secure", cookie, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("path=/api/auth", cookie, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("samesite=strict", cookie, StringComparison.OrdinalIgnoreCase);

        var refreshToken = cookie[(CookieName.Length + 1)..cookie.IndexOf(';')];
        var body = await response.Content.ReadAsStringAsync();
        Assert.DoesNotContain(refreshToken, body);
        Assert.Contains("accessToken", body);
    }

    /// <summary>A browser holding the cookie can obtain a new access token without sending a body.</summary>
    [Fact]
    public async Task Refresh_works_from_the_cookie_alone()
    {
        var patient = await Accounts.NewPatientAsync(factory);

        var response = await patient.Client.PostAsync("/api/auth/refresh", content: null);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        using var body = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        Assert.False(string.IsNullOrWhiteSpace(body.RootElement.GetProperty("accessToken").GetString()));
        Assert.StartsWith(CookieName + "=", SetCookieOf(response));
    }

    /// <summary>Without the cookie there is nothing to refresh.</summary>
    [Fact]
    public async Task Refresh_without_a_cookie_is_401()
    {
        var response = await factory.CreateApiClient().PostAsync("/api/auth/refresh", content: null);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    /// <summary>A refresh request that comes from another website is refused even with a valid cookie.</summary>
    [Fact]
    public async Task Refresh_from_a_foreign_origin_is_403()
    {
        var patient = await Accounts.NewPatientAsync(factory);
        using var request = new HttpRequestMessage(HttpMethod.Post, "/api/auth/refresh");
        request.Headers.Add("Origin", "https://evil.test");

        var response = await patient.Client.SendAsync(request);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    /// <summary>Refresh from the allowed web-client origin is accepted.</summary>
    [Fact]
    public async Task Refresh_from_the_allowed_origin_is_accepted()
    {
        var patient = await Accounts.NewPatientAsync(factory);
        using var request = new HttpRequestMessage(HttpMethod.Post, "/api/auth/refresh");
        request.Headers.Add("Origin", ApiFactory.AllowedOrigin);

        var response = await patient.Client.SendAsync(request);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    /// <summary>After logout the session cannot be refreshed any more.</summary>
    [Fact]
    public async Task Logout_ends_the_session()
    {
        var patient = await Accounts.NewPatientAsync(factory);

        var logout = await patient.Client.PostAsync("/api/auth/logout", content: null);
        var refresh = await patient.Client.PostAsync("/api/auth/refresh", content: null);

        Assert.Equal(HttpStatusCode.NoContent, logout.StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, refresh.StatusCode);
    }

    /// <summary>Login with the right password works; with a wrong one it is a generic 401.</summary>
    [Fact]
    public async Task Login_checks_the_password()
    {
        var doctor = await Accounts.NewDoctorAsync(factory);

        var good = await factory.CreateApiClient().PostAsJsonAsync(
            "/api/auth/login", new { email = doctor.Email, password = Accounts.Password });
        var bad = await factory.CreateApiClient().PostAsJsonAsync(
            "/api/auth/login", new { email = doctor.Email, password = "wrong-password" });

        Assert.Equal(HttpStatusCode.OK, good.StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, bad.StatusCode);
    }

    /// <summary>Registering the same national id twice is a 409 conflict with a stable code.</summary>
    [Fact]
    public async Task A_duplicate_national_id_is_409()
    {
        var first = await Accounts.NewPatientAsync(factory);

        var response = await factory.CreateApiClient().PostAsJsonAsync("/api/auth/signup/patient", new
        {
            fullName = "Duplicate Patient",
            phone = "01000000000",
            email = $"duplicate-{Guid.NewGuid():N}@example.com",
            password = "Passw0rd-123",
            age = 58,
            gender = "male",
            nationalId = first.NationalId,
            diagnosis = "Prior MI",
            preferredLanguage = "en"
        });

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        using var body = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        Assert.Equal("Patient.NationalIdTaken", body.RootElement.GetProperty("code").GetString());
    }
}
