// =============================================================================
// File           : WebApplicationExtensions.cs
// Layer          : Api
// Purpose        : The request pipeline (UseApi) and the start-up database step (InitializeDatabaseAsync).
// Logic          : Middleware order matters and is written down here once: errors are caught first, security headers
//                  are added to everything, routing runs before CORS and rate limiting (they are endpoint-aware),
//                  authentication before authorization, and the audit middleware runs last so it sees the real caller
//                  and the final status code.
// Implementation : Extension methods on WebApplication so Program.cs stays a few lines.
// =============================================================================
using CardioSense.Api.Middleware;
using CardioSense.Api.Options;
using CardioSense.Infrastructure.Persistence;
using Microsoft.Extensions.Options;

namespace CardioSense.Api;

/// <summary>Pipeline and start-up helpers.</summary>
public static class WebApplicationExtensions
{
    /// <summary>Prepares the database as configured: migrate or create the schema, then seed demo devices.</summary>
    public static async Task InitializeDatabaseAsync(this WebApplication app)
    {
        var mode = app.Services.GetRequiredService<IOptions<DatabaseOptions>>().Value.Initialization;

        switch (mode)
        {
            case DatabaseInitialization.Migrate:
                await DatabaseInitializer.MigrateAsync(app.Services);
                break;
            case DatabaseInitialization.EnsureCreated:
                await DatabaseInitializer.EnsureCreatedAsync(app.Services);
                break;
            default:
                return;
        }

        if (app.Environment.IsDevelopment())
        {
            await DatabaseInitializer.SeedDemoDevicesAsync(app.Services);
        }
    }

    /// <summary>Builds the middleware pipeline in the required order.</summary>
    public static WebApplication UseApi(this WebApplication app)
    {
        app.UseExceptionHandler();
        app.UseStatusCodePages();
        app.UseMiddleware<SecurityHeadersMiddleware>();

        app.UseRouting();
        app.UseCors();
        app.UseRateLimiter();
        app.UseAuthentication();
        app.UseAuthorization();
        app.UseMiddleware<AuditLoggingMiddleware>();

        return app;
    }
}
