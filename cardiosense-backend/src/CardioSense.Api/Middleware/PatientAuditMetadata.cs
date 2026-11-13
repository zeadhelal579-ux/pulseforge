// =============================================================================
// File           : PatientAuditMetadata.cs
// Layer          : Api / Middleware
// Purpose        : Marker that tells AuditLoggingMiddleware "this route reads or writes a patient's record".
// Logic          : Auditing is opt-in per route group through endpoint metadata, so the middleware needs no list of
//                  URLs and a new patient route is audited just by living in an audited group.
// Implementation : Stateless singleton attached with WithMetadata.
// =============================================================================
namespace CardioSense.Api.Middleware;

/// <summary>Marks an endpoint as access to a patient's record.</summary>
internal sealed class PatientAuditMetadata
{
    /// <summary>The only instance.</summary>
    public static readonly PatientAuditMetadata Instance = new();
}
