// =============================================================================
// File           : DatabaseInitializer.cs
// Layer          : Infrastructure / Persistence
// Purpose        : Start-up database tasks called by the Api host: create the schema and seed demo devices.
// Logic          : Device pairing works only for devices that already exist in the inventory (there is no admin role
//                  in Phase 1), so Development needs a few inventory devices to pair against. Seeding is skipped as
//                  soon as any device exists, which makes it safe to run on every start.
// Implementation : Static helpers that open their own scope. MigrateAsync needs at least one migration to exist
//                  (generate it with the command shown in AppDbContextFactory); EnsureCreatedAsync builds the schema
//                  straight from the model so a brand-new machine or test database works before any migration does.
// =============================================================================
using CardioSense.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace CardioSense.Infrastructure.Persistence;

/// <summary>Start-up helpers for the database.</summary>
public static class DatabaseInitializer
{
    /// <summary>How many demo devices the Development seed registers.</summary>
    private const int DemoDeviceCount = 10;

    /// <summary>Applies every pending migration.</summary>
    public static async Task MigrateAsync(IServiceProvider services, CancellationToken ct = default)
    {
        await using var scope = services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await db.Database.MigrateAsync(ct);
    }

    /// <summary>
    /// Creates the schema from the current model when the database is empty. For local development and tests only:
    /// a database created this way cannot be moved to migrations later, so production always uses MigrateAsync.
    /// </summary>
    public static async Task EnsureCreatedAsync(IServiceProvider services, CancellationToken ct = default)
    {
        await using var scope = services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await db.Database.EnsureCreatedAsync(ct);
    }

    /// <summary>Registers demo inventory devices (CS-DEMO-0001...) when none exist. Development only.</summary>
    public static async Task SeedDemoDevicesAsync(IServiceProvider services, CancellationToken ct = default)
    {
        await using var scope = services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        if (await db.Devices.AnyAsync(ct))
        {
            return;
        }

        var now = scope.ServiceProvider.GetRequiredService<TimeProvider>().GetUtcNow();
        for (var number = 1; number <= DemoDeviceCount; number++)
        {
            db.Devices.Add(Device.Register($"CS-DEMO-{number:D4}", now));
        }

        await db.SaveChangesAsync(ct);
    }
}
