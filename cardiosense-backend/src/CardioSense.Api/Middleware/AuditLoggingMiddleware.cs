// =============================================================================
// File           : AuditLoggingMiddleware.cs
// Layer          : Api / Middleware
// Purpose        : Writes one AuditLogs row for every request that touches a patient's record.
// Logic          : Answers "who looked at this patient's file, when and from where" after the fact - a real
//                  requirement once real medical data is in the system. The row is written after the response status
//                  is known, so refused attempts (403) are recorded too, which is exactly what an investigation
//                  needs. The patient is the {id} in the route, or the caller's own patient id for /me routes.
//                  A failure to write the audit row is logged but never changes the response the client already got.
// Implementation : Conventional middleware; only endpoints carrying PatientAuditMetadata are audited.
// =============================================================================
using CardioSense.Application.Common.Interfaces;
using CardioSense.Domain.Entities;

namespace CardioSense.Api.Middleware;

/// <summary>Records access to patient records in the audit log.</summary>
internal sealed class AuditLoggingMiddleware(RequestDelegate next, ILogger<AuditLoggingMiddleware> logger)
{
    /// <summary>The audited entity type.</summary>
    private const string PatientEntityType = "Patient";

    /// <summary>Name of the route value that holds a patient id.</summary>
    private const string PatientIdRouteKey = "id";

    /// <summary>Runs the request, then writes the audit row when the route is marked for auditing.</summary>
    public async Task InvokeAsync(
        HttpContext context, IAppDbContext db, ICurrentUser currentUser, TimeProvider clock)
    {
        await next(context);

        var endpoint = context.GetEndpoint();
        if (endpoint?.Metadata.GetMetadata<PatientAuditMetadata>() is null)
        {
            return;
        }

        try
        {
            var routePattern = (endpoint as RouteEndpoint)?.RoutePattern.RawText ?? context.Request.Path.Value;
            var action = $"{context.Request.Method} {routePattern} -> {context.Response.StatusCode}";
            var patientId = Guid.TryParse(context.Request.RouteValues[PatientIdRouteKey]?.ToString(), out var routed)
                ? routed
                : currentUser.PatientId;

            db.AuditLogEntries.Add(AuditLogEntry.Create(
                currentUser.UserId, action, PatientEntityType, patientId,
                context.Connection.RemoteIpAddress?.ToString(), clock.GetUtcNow()));

            // The client may already be gone; the audit row must still be saved.
            await db.SaveChangesAsync(CancellationToken.None);
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "Could not write the audit row for {Method} {Path}.",
                context.Request.Method, context.Request.Path);
        }
    }
}
