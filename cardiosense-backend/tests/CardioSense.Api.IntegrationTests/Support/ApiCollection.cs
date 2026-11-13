// =============================================================================
// File           : ApiCollection.cs
// Layer          : Tests / Support
// Purpose        : Makes every integration test class share one ApiFactory (one container, one API instance).
// Logic          : xUnit creates the fixture once per collection; every class tagged with the collection reuses it.
// Implementation : A collection definition with the fixture; it contains no code.
// =============================================================================
namespace CardioSense.Api.IntegrationTests.Support;

/// <summary>The collection of tests sharing the API and its database.</summary>
[CollectionDefinition(Name)]
public sealed class ApiCollection : ICollectionFixture<ApiFactory>
{
    /// <summary>The collection name used by [Collection] on test classes.</summary>
    public const string Name = "api";
}
