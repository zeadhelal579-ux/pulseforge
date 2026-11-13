// =============================================================================
// File           : Program.cs
// Layer          : Api
// Purpose        : Entry point and composition root of the CardioSense API.
// Logic          : Registers the three layers (Application first, because it registers the shared TimeProvider that
//                  Infrastructure also uses), prepares the database, builds the pipeline and maps every endpoint
//                  group. Nothing else belongs here: a new endpoint is a new IEndpointGroup class, not a change to
//                  this file.
// Implementation : Top-level statements. The partial Program class at the bottom is public only so integration tests
//                  can host the whole API in memory.
// =============================================================================
using CardioSense.Api;
using CardioSense.Api.Endpoints;
using CardioSense.Application;
using CardioSense.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddApi();

var app = builder.Build();

await app.InitializeDatabaseAsync();

app.UseApi();
app.MapEndpointGroups();

await app.RunAsync();

/// <summary>Entry-point type; public so integration tests can reference it.</summary>
public partial class Program;
