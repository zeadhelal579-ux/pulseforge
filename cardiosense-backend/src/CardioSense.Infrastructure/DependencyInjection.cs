// =============================================================================
// File           : DependencyInjection.cs
// Layer          : Infrastructure
// Purpose        : AddInfrastructure() - registers every concrete implementation behind the Application interfaces.
// Logic          : One call from the Api's Program.cs wires the database, Identity, tokens, the vitals store, the AI
//                  client, SignalR and the MQTT worker. Settings are validated when the host starts, so a missing
//                  connection string or signing key fails immediately with a clear message, not on the first request.
//                  Swapping a technology (for example the time-series store) means changing one registration here.
// Implementation : Extension method on IServiceCollection. No retry-on-failure execution strategy is enabled for
//                  SQL Server because the registration handlers use explicit transactions (they are incompatible
//                  with it unless every transaction is wrapped in an execution strategy).
// =============================================================================
using System.Text.Json.Serialization;
using CardioSense.Application.Common.Interfaces;
using CardioSense.Application.Common.Options;
using CardioSense.Domain.Interfaces;
using CardioSense.Infrastructure.AiClient;
using CardioSense.Infrastructure.Identity;
using CardioSense.Infrastructure.Ingestion;
using CardioSense.Infrastructure.Persistence;
using CardioSense.Infrastructure.Realtime;
using CardioSense.Infrastructure.TimeSeries;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace CardioSense.Infrastructure;

/// <summary>Infrastructure layer registration.</summary>
public static class DependencyInjection
{
    /// <summary>Name of the connection string that points at the SQL Server database.</summary>
    private const string ConnectionStringName = "DefaultConnection";

    /// <summary>Registers persistence, identity, tokens, time series, AI client, live push and ingestion.</summary>
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services, IConfiguration configuration)
    {
        BindOptions(services, configuration);
        AddPersistence(services, configuration);
        AddAccounts(services);

        services.AddScoped<IVitalsTimeSeriesStore, SqlVitalsTimeSeriesStore>();

        services.AddHttpClient<IRiskAssessmentService, HttpRiskAssessmentService>((provider, client) =>
        {
            var settings = provider.GetRequiredService<IOptions<AiServiceOptions>>().Value;
            client.BaseAddress = new Uri(settings.BaseUrl.TrimEnd('/') + "/");
            client.Timeout = TimeSpan.FromSeconds(settings.TimeoutSeconds);
        });

        // Enums travel as names (Stable, Critical, WiFi) so the TypeScript union types match.
        services.AddSignalR().AddJsonProtocol(protocol =>
            protocol.PayloadSerializerOptions.Converters.Add(new JsonStringEnumConverter()));
        services.AddSingleton<IRealtimeNotifier, SignalRRealtimeNotifier>();

        services.AddHostedService<MqttIngestionService>();

        return services;
    }

    /// <summary>Binds and validates every configuration section used by this layer.</summary>
    private static void BindOptions(IServiceCollection services, IConfiguration configuration)
    {
        services.AddOptions<JwtOptions>()
            .Bind(configuration.GetSection(JwtOptions.SectionName))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services.AddOptions<AiServiceOptions>()
            .Bind(configuration.GetSection(AiServiceOptions.SectionName))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services.AddOptions<MqttOptions>()
            .Bind(configuration.GetSection(MqttOptions.SectionName))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services.Configure<MonitoringOptions>(configuration.GetSection(MonitoringOptions.SectionName));
    }

    /// <summary>Registers the SQL Server context and exposes it through the Application interface.</summary>
    private static void AddPersistence(IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString(ConnectionStringName)
            ?? throw new InvalidOperationException(
                $"The connection string '{ConnectionStringName}' is missing (ConnectionStrings section).");

        services.AddDbContext<AppDbContext>(builder => builder.UseSqlServer(connectionString));
        services.AddScoped<IAppDbContext>(provider => provider.GetRequiredService<AppDbContext>());
    }

    /// <summary>Registers ASP.NET Core Identity (accounts, passwords, lockout) and the token service.</summary>
    private static void AddAccounts(IServiceCollection services)
    {
        services.AddIdentityCore<ApplicationUser>(identity =>
            {
                // Same rule as the frontend form: at least 8 characters, no composition requirements.
                identity.Password.RequiredLength = 8;
                identity.Password.RequireDigit = false;
                identity.Password.RequireLowercase = false;
                identity.Password.RequireUppercase = false;
                identity.Password.RequireNonAlphanumeric = false;

                identity.User.RequireUniqueEmail = true;

                identity.Lockout.AllowedForNewUsers = true;
                identity.Lockout.MaxFailedAccessAttempts = 5;
                identity.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
            })
            .AddEntityFrameworkStores<AppDbContext>();

        services.AddScoped<IIdentityService, IdentityService>();
        services.AddSingleton<ITokenService, JwtTokenService>();
    }
}
