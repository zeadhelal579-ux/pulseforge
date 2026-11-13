// =============================================================================
// File           : ApiFactory.cs
// Layer          : Tests / Support
// Purpose        : Starts the real API in memory on top of a throw-away SQL Server container.
// Logic          : One container and one API instance are shared by all integration tests (starting SQL Server is
//                  slow), so each test creates its own users with unique ids. Settings are passed as environment
//                  variables because they win over appsettings files and are read by the host before Program.cs
//                  runs - which also lets the test run exercise the real start-up path (EnsureCreated + demo devices).
// Implementation : WebApplicationFactory over Program, Testcontainers.MsSql for the database, the AI service
//                  replaced by StubAiService, xUnit IAsyncLifetime to start and stop the container.
// =============================================================================
using CardioSense.Application.Common.Interfaces;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Testcontainers.MsSql;

namespace CardioSense.Api.IntegrationTests.Support;

/// <summary>The API under test plus its database.</summary>
public sealed class ApiFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    /// <summary>The SQL Server container.</summary>
    private MsSqlContainer? _database;

    /// <summary>Origin the frontend would call from; it is on the CORS allow-list.</summary>
    public const string AllowedOrigin = "https://app.test";

    /// <summary>Starts SQL Server and publishes the settings the API reads at start-up.</summary>
    public async Task InitializeAsync()
    {
        var connectionString = Environment.GetEnvironmentVariable("CARDIOSENSE_TEST_SQL");
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            _database = new MsSqlBuilder().Build();
            await _database.StartAsync();
            connectionString = _database.GetConnectionString();
        }

        var settings = new Dictionary<string, string>
        {
            ["ConnectionStrings__DefaultConnection"] = connectionString,
            ["Jwt__Issuer"] = "CardioSense.Tests",
            ["Jwt__Audience"] = "CardioSense.Tests.Web",
            ["Jwt__SigningKey"] = "integration-test-signing-key-0123456789-abcdef",
            ["Mqtt__Enabled"] = "false",
            ["Database__Initialization"] = "EnsureCreated",
            ["Cors__AllowedOrigins__0"] = AllowedOrigin,
            ["RateLimiting__AuthPermitLimit"] = "1000"
        };

        foreach (var (key, value) in settings)
        {
            Environment.SetEnvironmentVariable(key, value);
        }
    }

    /// <summary>Stops the API and the database container.</summary>
    async Task IAsyncLifetime.DisposeAsync()
    {
        await base.DisposeAsync();
        if (_database is not null)
        {
            await _database.DisposeAsync();
        }
    }

    /// <summary>A client that talks HTTPS (the refresh cookie is Secure) and keeps cookies like a browser.</summary>
    public HttpClient CreateApiClient() => CreateClient(new WebApplicationFactoryClientOptions
    {
        BaseAddress = new Uri("https://localhost"),
        HandleCookies = true,
        AllowAutoRedirect = false
    });

    /// <summary>Swaps the AI service for the stub; everything else stays real.</summary>
    protected override void ConfigureWebHost(IWebHostBuilder builder) =>
        builder.ConfigureTestServices(services =>
        {
            services.RemoveAll<IRiskAssessmentService>();
            services.AddSingleton<IRiskAssessmentService, StubAiService>();
        });
}
