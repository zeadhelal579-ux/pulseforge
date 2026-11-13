// =============================================================================
// File           : ApplicationTestHost.cs
// Layer          : Tests / Support
// Purpose        : Builds the real Application (mediator pipeline, validators, handlers) on top of fakes.
// Logic          : Tests send commands and queries through ISender exactly like an endpoint does, so authorization,
//                  validation and the handler run together. Every port Infrastructure would implement is replaced by
//                  a fake registered AFTER AddApplication, so the fake wins. Each host owns a fresh in-memory database.
// Implementation : One ServiceProvider per host; every call runs in its own scope (like one HTTP request).
// =============================================================================
using CardioSense.Application.Common.Interfaces;
using CardioSense.Domain.Interfaces;
using Mediator;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.DependencyInjection;

namespace CardioSense.Application.Tests.Support;

/// <summary>A self-contained application instance for one test.</summary>
public sealed class ApplicationTestHost : IAsyncDisposable
{
    /// <summary>The fixed instant every test starts at.</summary>
    public static readonly DateTimeOffset Start = new(2026, 11, 1, 12, 0, 0, TimeSpan.Zero);

    /// <summary>The service provider that owns everything.</summary>
    private readonly ServiceProvider _provider;

    /// <summary>Creates the host with a clean database and default fakes.</summary>
    public ApplicationTestHost()
    {
        var databaseName = Guid.NewGuid().ToString();
        var services = new ServiceCollection();

        services.AddLogging();
        services.AddApplication();

        services.AddDbContext<TestDbContext>(options => options
            .UseInMemoryDatabase(databaseName)
            .ConfigureWarnings(warnings => warnings.Ignore(InMemoryEventId.TransactionIgnoredWarning)));
        services.AddScoped<IAppDbContext>(provider => provider.GetRequiredService<TestDbContext>());

        services.AddSingleton<TimeProvider>(Clock);
        services.AddSingleton<ICurrentUser>(CurrentUser);
        services.AddSingleton<IRealtimeNotifier>(Notifier);
        services.AddSingleton<IRiskAssessmentService>(RiskService);
        services.AddSingleton<IVitalsTimeSeriesStore>(Vitals);
        services.AddSingleton<IIdentityService>(Identity);
        services.AddSingleton<ITokenService>(Tokens);

        _provider = services.BuildServiceProvider();
    }

    /// <summary>The clock handlers read.</summary>
    public FixedTimeProvider Clock { get; } = new(Start);

    /// <summary>The signed-in user.</summary>
    public FakeCurrentUser CurrentUser { get; } = new();

    /// <summary>Real-time pushes recorded.</summary>
    public RecordingNotifier Notifier { get; } = new();

    /// <summary>The scripted AI service.</summary>
    public StubRiskAssessmentService RiskService { get; } = new();

    /// <summary>The vitals store.</summary>
    public InMemoryVitalsStore Vitals { get; } = new();

    /// <summary>The account store.</summary>
    public StubIdentityService Identity { get; } = new();

    /// <summary>The token generator.</summary>
    public StubTokenService Tokens { get; } = new();

    /// <summary>Sends a request through the full pipeline in its own scope.</summary>
    public async Task<TResponse> SendAsync<TResponse>(IRequest<TResponse> request)
    {
        await using var scope = _provider.CreateAsyncScope();
        var sender = scope.ServiceProvider.GetRequiredService<ISender>();
        return await sender.Send(request);
    }

    /// <summary>Saves ready-made entities to the database.</summary>
    public Task SeedAsync(params object[] entities) => SeedAsync(db =>
    {
        db.AddRange(entities);
        return Task.CompletedTask;
    });

    /// <summary>Runs setup code against the database (committed before it returns).</summary>
    public async Task SeedAsync(Func<TestDbContext, Task> seed)
    {
        await using var scope = _provider.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<TestDbContext>();
        await seed(db);
        await db.SaveChangesAsync();
    }

    /// <summary>Reads from the database in its own scope.</summary>
    public async Task<T> QueryAsync<T>(Func<TestDbContext, Task<T>> query)
    {
        await using var scope = _provider.CreateAsyncScope();
        return await query(scope.ServiceProvider.GetRequiredService<TestDbContext>());
    }

    /// <summary>Disposes the service provider and its in-memory database.</summary>
    public ValueTask DisposeAsync() => _provider.DisposeAsync();
}
