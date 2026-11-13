// =============================================================================
// File           : AppDbContextFactory.cs
// Layer          : Infrastructure / Persistence
// Purpose        : Lets the "dotnet ef" tools create the context without starting the whole application.
// Logic          : Migrations can be generated and applied with only this project (no Api needed):
//                    dotnet ef migrations add InitialCreate --project src/CardioSense.Infrastructure
//                    dotnet ef database update --project src/CardioSense.Infrastructure
//                  The connection string comes from the CARDIOSENSE_CONNECTION_STRING environment variable; the
//                  LocalDB default exists for tooling only and is never used at runtime.
// Implementation : IDesignTimeDbContextFactory discovered by the EF tools through reflection.
// =============================================================================
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace CardioSense.Infrastructure.Persistence;

/// <summary>Design-time factory used by the EF Core command-line tools.</summary>
public sealed class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    /// <summary>Environment variable that overrides the tooling connection string.</summary>
    private const string ConnectionStringVariable = "CARDIOSENSE_CONNECTION_STRING";

    /// <summary>Local development database used when the variable is not set.</summary>
    private const string LocalDefaultConnectionString =
        "Server=(localdb)\\MSSQLLocalDB;Database=CardioSense;Trusted_Connection=True;TrustServerCertificate=True";

    /// <inheritdoc />
    public AppDbContext CreateDbContext(string[] args)
    {
        var connectionString = Environment.GetEnvironmentVariable(ConnectionStringVariable)
                               ?? LocalDefaultConnectionString;

        var options = new DbContextOptionsBuilder<AppDbContext>().UseSqlServer(connectionString).Options;
        return new AppDbContext(options);
    }
}
