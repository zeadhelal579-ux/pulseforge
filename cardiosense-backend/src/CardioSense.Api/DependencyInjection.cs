// =============================================================================
// File           : DependencyInjection.cs
// Layer          : Api
// Purpose        : AddApi(): registers everything the HTTP host itself needs, in one call from Program.cs.
// Logic          : Grouped by concern - options, authentication/authorization, browser access (CORS, rate limiting,
//                  cookies), JSON and error handling, endpoints. All settings are read through validated IOptions, so
//                  a missing or invalid value stops the host at start-up with a clear message instead of failing on the
//                  first request. Application and Infrastructure are registered by their own AddApplication /
//                  AddInfrastructure; this file only wires the Api layer to them.
// Implementation : One public extension method that calls small private ones.
// =============================================================================
using System.Reflection;
using System.Text.Json.Serialization;
using System.Threading.RateLimiting;
using CardioSense.Api.Authentication;
using CardioSense.Api.Authorization;
using CardioSense.Api.Endpoints;
using CardioSense.Api.Middleware;
using CardioSense.Api.Options;
using CardioSense.Application.Common.Interfaces;
using CardioSense.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors.Infrastructure;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Options;

namespace CardioSense.Api;

/// <summary>Service registration for the Api layer.</summary>
public static class DependencyInjection
{
    /// <summary>Registers options, security, HTTP behaviour and the endpoint groups.</summary>
    public static IServiceCollection AddApi(this IServiceCollection services)
    {
        AddApiOptions(services);
        AddSecurity(services);
        AddBrowserAccess(services);
        AddHttpBehaviour(services);

        services.AddEndpointGroups(Assembly.GetExecutingAssembly());
        return services;
    }

    /// <summary>Binds and validates the Api settings sections.</summary>
    private static void AddApiOptions(IServiceCollection services)
    {
        services.AddOptions<ApiCorsOptions>()
            .BindConfiguration(ApiCorsOptions.SectionName).ValidateDataAnnotations().ValidateOnStart();
        services.AddOptions<RefreshCookieOptions>()
            .BindConfiguration(RefreshCookieOptions.SectionName).ValidateDataAnnotations().ValidateOnStart();
        services.AddOptions<RateLimitOptions>()
            .BindConfiguration(RateLimitOptions.SectionName).ValidateDataAnnotations().ValidateOnStart();
        services.AddOptions<DatabaseOptions>().BindConfiguration(DatabaseOptions.SectionName);
    }

    /// <summary>Authentication (JWT bearer), authorization policies and the current-user accessor.</summary>
    private static void AddSecurity(IServiceCollection services)
    {
        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer();
        services.ConfigureOptions<JwtBearerSetup>();
        services.AddRolePolicies();
        services.AddScoped<IAuthorizationHandler, PatientAccessHandler>();

        services.AddHttpContextAccessor();
        services.AddScoped<CurrentUser>();
        services.AddScoped<ICurrentUser>(provider => provider.GetRequiredService<CurrentUser>());
        services.AddSignalR(hub => hub.AddFilter<CurrentUserHubFilter>());

        services.AddSingleton<RefreshCookie>();
    }

    /// <summary>CORS for the web client and rate limiting for the guessable auth endpoints.</summary>
    private static void AddBrowserAccess(IServiceCollection services)
    {
        services.AddCors();
        services.AddOptions<CorsOptions>().Configure<IOptions<ApiCorsOptions>>((cors, allowed) =>
            cors.AddDefaultPolicy(policy => policy
                .WithOrigins(allowed.Value.AllowedOrigins)
                .AllowAnyHeader()
                .AllowAnyMethod()
                .AllowCredentials()));

        services.AddRateLimiter(_ => { });
        services.AddOptions<RateLimiterOptions>().Configure<IOptions<RateLimitOptions>>((limiter, settings) =>
        {
            limiter.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
            limiter.AddPolicy(ApiRoutes.AuthRateLimit, context => RateLimitPartition.GetFixedWindowLimiter(
                context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                _ => new FixedWindowRateLimiterOptions
                {
                    PermitLimit = settings.Value.AuthPermitLimit,
                    Window = TimeSpan.FromSeconds(settings.Value.AuthWindowSeconds)
                }));
        });
    }

    /// <summary>JSON shape, error responses, health checks and the OpenAPI document.</summary>
    private static void AddHttpBehaviour(IServiceCollection services)
    {
        // Enums travel as their names ("Stable", "WiFi"), exactly like the TypeScript unions in the frontend.
        services.ConfigureHttpJsonOptions(json => json.SerializerOptions.Converters.Add(new JsonStringEnumConverter()));

        services.AddProblemDetails();
        services.AddExceptionHandler<GlobalExceptionHandler>();

        services.AddHealthChecks().AddDbContextCheck<AppDbContext>(tags: [HealthEndpoints.ReadyTag]);
        services.AddOpenApi();
    }
}
