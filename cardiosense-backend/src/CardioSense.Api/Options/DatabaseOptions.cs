// =============================================================================
// File           : DatabaseOptions.cs
// Layer          : Api / Options
// Purpose        : Start-up database settings.
// Logic          : See DatabaseInitialization; kept in its own options class so the choice is explicit per
//                  environment (appsettings.Development.json, docker-compose, production environment variables).
// Implementation : Bound from the "Database" configuration section.
// =============================================================================
namespace CardioSense.Api.Options;

/// <summary>Database start-up settings.</summary>
public sealed class DatabaseOptions
{
    /// <summary>Configuration section name.</summary>
    public const string SectionName = "Database";

    /// <summary>What to do with the database at start-up.</summary>
    public DatabaseInitialization Initialization { get; set; } = DatabaseInitialization.None;
}
