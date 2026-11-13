// =============================================================================
// File           : DatabaseInitialization.cs
// Layer          : Api / Options
// Purpose        : What the host does to the database when it starts.
// Logic          : Production applies migrations; local development and tests can build the schema straight from
//                  the model; None leaves the database alone (the safe default for any environment that forgot to
//                  decide).
// Implementation : Plain enum, parsed from configuration by name.
// =============================================================================
namespace CardioSense.Api.Options;

/// <summary>Start-up database behaviour.</summary>
public enum DatabaseInitialization
{
    /// <summary>Do nothing.</summary>
    None = 0,

    /// <summary>Apply pending EF Core migrations.</summary>
    Migrate = 1,

    /// <summary>Create the schema from the model (development and tests only).</summary>
    EnsureCreated = 2
}
