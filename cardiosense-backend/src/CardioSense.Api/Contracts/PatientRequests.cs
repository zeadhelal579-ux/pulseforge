// =============================================================================
// File           : PatientRequests.cs
// Layer          : Api / Contracts
// Purpose        : JSON request bodies of the patient endpoints that cannot bind straight to an Application command.
// Logic          : The command carries the patient id and the caller's identity, which come from the URL or the
//                  token and must never be taken from the body. So the body type holds only what the client sends and
//                  the endpoint combines it with the id. Field names match the frontend request types.
// Implementation : Immutable records, bound by System.Text.Json (camelCase, enums as names).
// =============================================================================
using CardioSense.Domain.Enums;

namespace CardioSense.Api.Contracts;

/// <summary>Body of PUT /api/patients/me (a null field is left unchanged).</summary>
public sealed record UpdatePatientProfileRequest(string? FullName, string? Phone, string? PreferredLanguage);

/// <summary>Body of POST /api/patients/{id}/manual-events.</summary>
public sealed record LogManualEventRequest(string Note, AlertSeverity Severity);

/// <summary>Body of POST /api/patients/{id}/freeze-waveform.</summary>
public sealed record FreezeWaveformRequest(bool Frozen);

/// <summary>Body of POST /api/patients/{id}/alerts/{alertId}/resolve (the whole body is optional).</summary>
public sealed record ResolveAlertRequest(string? ResolutionNote);
