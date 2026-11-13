// =============================================================================
// File           : PatientEndpoints.cs
// Layer          : Api / Endpoints
// Purpose        : /api/patients - a patient's own data (/me) and a doctor's view of an assigned patient (/{id}).
// Logic          : Both views serve the same shared detail screen, so every read exists twice with the same logic: the
//                  /me variants (patients only) resolve the id from the caller's token, the /{id} variants (doctors
//                  only) take it from the URL and are additionally guarded by PatientAccessFilter so a doctor can
//                  only reach assigned patients. The clinical actions - manual event, freeze waveform, resolve alert -
//                  exist only under /{id}, which is how they stay hidden from patients. Both groups are audited.
// Implementation : Each endpoint is one mediator call; the /me handlers just supply the caller's own patient id and
//                  reuse the /{id} handler.
// =============================================================================
using CardioSense.Api.Authorization;
using CardioSense.Api.Contracts;
using CardioSense.Api.Http;
using CardioSense.Api.Middleware;
using CardioSense.Application.Common.Contracts;
using CardioSense.Application.Common.Errors;
using CardioSense.Application.Common.Interfaces;
using CardioSense.Application.Patients.Commands.FreezeWaveform;
using CardioSense.Application.Patients.Commands.LogManualEvent;
using CardioSense.Application.Patients.Commands.ResolveAlert;
using CardioSense.Application.Patients.Commands.UpdatePatientProfile;
using CardioSense.Application.Patients.Queries.GetPatientAiReport;
using CardioSense.Application.Patients.Queries.GetPatientAlerts;
using CardioSense.Application.Patients.Queries.GetPatientProfile;
using CardioSense.Application.Patients.Queries.GetPatientVitalsHistory;
using Mediator;

namespace CardioSense.Api.Endpoints;

/// <summary>Patient data endpoints.</summary>
internal sealed class PatientEndpoints : IEndpointGroup
{
    /// <summary>Maps the /me routes (patients) and the /{id} routes (assigned doctors).</summary>
    public void Map(IEndpointRouteBuilder app)
    {
        var patients = app.MapGroup("/api/patients").WithTags("Patients").RequireAuthorization();

        var me = patients.MapGroup("/me")
            .RequireAuthorization(AuthorizationPolicies.PatientOnly)
            .WithMetadata(PatientAuditMetadata.Instance);
        me.MapGet("/", GetMyProfile);
        me.MapPut("/", UpdateMyProfile);
        me.MapGet("/vitals-history", GetMyVitalsHistory);
        me.MapGet("/alerts", GetMyAlerts);
        me.MapGet("/ai-report", GetMyAiReport);

        var byId = patients.MapGroup("/{id:guid}")
            .RequireAuthorization(AuthorizationPolicies.DoctorOnly)
            .AddEndpointFilter<PatientAccessFilter>()
            .WithMetadata(PatientAuditMetadata.Instance);
        byId.MapGet("/", GetProfile);
        byId.MapGet("/vitals-history", GetVitalsHistory);
        byId.MapGet("/alerts", GetAlerts);
        byId.MapGet("/ai-report", GetAiReport);
        byId.MapPost("/manual-events", LogManualEvent);
        byId.MapPost("/freeze-waveform", FreezeWaveform);
        byId.MapPost("/alerts/{alertId:guid}/resolve", ResolveAlert);
    }

    // ---- /me: the caller's own patient -------------------------------------------------------------------------

    /// <summary>GET /api/patients/me.</summary>
    private static Task<IResult> GetMyProfile(ICurrentUser user, ISender sender, CancellationToken ct) =>
        AsOwner(user, patientId => GetProfile(patientId, sender, ct));

    /// <summary>PUT /api/patients/me - partial update of the caller's own profile.</summary>
    private static Task<IResult> UpdateMyProfile(
        UpdatePatientProfileRequest body, ICurrentUser user, ISender sender, CancellationToken ct) =>
        AsOwner(user, async patientId => (await sender.Send(
            new UpdatePatientProfileCommand(patientId, body.FullName, body.Phone, body.PreferredLanguage), ct))
            .ToHttpResult());

    /// <summary>GET /api/patients/me/vitals-history?range=.</summary>
    private static Task<IResult> GetMyVitalsHistory(
        string? range, ICurrentUser user, ISender sender, CancellationToken ct) =>
        AsOwner(user, patientId => GetVitalsHistory(patientId, range, sender, ct));

    /// <summary>GET /api/patients/me/alerts.</summary>
    private static Task<IResult> GetMyAlerts(ICurrentUser user, ISender sender, CancellationToken ct) =>
        AsOwner(user, patientId => GetAlerts(patientId, sender, ct));

    /// <summary>GET /api/patients/me/ai-report?period=.</summary>
    private static Task<IResult> GetMyAiReport(
        string? period, ICurrentUser user, ISender sender, CancellationToken ct) =>
        AsOwner(user, patientId => GetAiReport(patientId, period, sender, ct));

    // ---- /{id}: an assigned patient, seen by a doctor ----------------------------------------------------------

    /// <summary>GET /api/patients/{id}.</summary>
    private static async Task<IResult> GetProfile(Guid id, ISender sender, CancellationToken ct) =>
        (await sender.Send(new GetPatientProfileQuery(id), ct)).ToHttpResult();

    /// <summary>GET /api/patients/{id}/vitals-history?range= (24h when omitted).</summary>
    private static async Task<IResult> GetVitalsHistory(
        Guid id, string? range, ISender sender, CancellationToken ct) =>
        (await sender.Send(
            new GetPatientVitalsHistoryQuery(id, range ?? ContractLiterals.VitalsRanges.Day), ct)).ToHttpResult();

    /// <summary>GET /api/patients/{id}/alerts.</summary>
    private static async Task<IResult> GetAlerts(Guid id, ISender sender, CancellationToken ct) =>
        (await sender.Send(new GetPatientAlertsQuery(id), ct)).ToHttpResult();

    /// <summary>GET /api/patients/{id}/ai-report?period= (daily when omitted).</summary>
    private static async Task<IResult> GetAiReport(Guid id, string? period, ISender sender, CancellationToken ct) =>
        (await sender.Send(
            new GetPatientAiReportQuery(id, period ?? ContractLiterals.ReportPeriods.Daily), ct)).ToHttpResult();

    /// <summary>POST /api/patients/{id}/manual-events - "Log Manual Event".</summary>
    private static async Task<IResult> LogManualEvent(
        Guid id, LogManualEventRequest body, ISender sender, CancellationToken ct) =>
        (await sender.Send(new LogManualEventCommand(id, body.Note, body.Severity), ct))
        .ToCreatedResult($"/api/patients/{id}/alerts");

    /// <summary>POST /api/patients/{id}/freeze-waveform - "Freeze Waveform".</summary>
    private static async Task<IResult> FreezeWaveform(
        Guid id, FreezeWaveformRequest body, ISender sender, CancellationToken ct) =>
        (await sender.Send(new FreezeWaveformCommand(id, body.Frozen), ct)).ToNoContentResult();

    /// <summary>POST /api/patients/{id}/alerts/{alertId}/resolve - a doctor resolves an alert.</summary>
    private static async Task<IResult> ResolveAlert(
        Guid id, Guid alertId, ResolveAlertRequest? body, ISender sender, CancellationToken ct) =>
        (await sender.Send(new ResolveAlertCommand(id, alertId, body?.ResolutionNote), ct)).ToHttpResult();

    /// <summary>Runs an action for the caller's own patient id, or answers 403 when the caller has none.</summary>
    private static Task<IResult> AsOwner(ICurrentUser user, Func<Guid, Task<IResult>> run) =>
        user.PatientId is { } patientId
            ? run(patientId)
            : Task.FromResult(ApplicationErrors.Patient.AccessDenied.ToProblem());
}
