// =============================================================================
// File           : SignalRRealtimeNotifier.cs
// Layer          : Infrastructure / Realtime
// Purpose        : Implements IRealtimeNotifier - pushes live data to browsers through the monitoring hub.
// Logic          : Vitals, ECG samples and alerts go to everyone watching the patient (the patient and the viewing
//                  doctors). The risk status goes out in two variants: the clinical one only to the doctors group and
//                  the patient-safe one (status only, no numbers) only to the patient. Patient-list summaries go
//                  to each doctor's personal group. Pushing is best-effort: a transport failure is logged and
//                  never breaks the telemetry pipeline that called it.
// Implementation : Singleton over IHubContext. ECG samples are sent one message per sample because the frontend
//                  handler receives a single number; sending stops at the first failure so a dead connection does not
//                  produce hundreds of identical log lines.
// =============================================================================
using CardioSense.Application.Common.Dtos;
using CardioSense.Application.Common.Interfaces;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;

namespace CardioSense.Infrastructure.Realtime;

/// <summary>SignalR-backed live push.</summary>
internal sealed class SignalRRealtimeNotifier(IHubContext<MonitoringHub> hub, ILogger<SignalRRealtimeNotifier> logger)
    : IRealtimeNotifier
{
    /// <inheritdoc />
    public Task NotifyVitalsAsync(VitalsTickPayload payload, CancellationToken ct = default) =>
        TrySendAsync(EveryViewerOf(payload.PatientId), MonitoringNames.VitalsEvent(payload.PatientId), payload, ct);

    /// <inheritdoc />
    public async Task NotifyEcgSamplesAsync(
        Guid patientId, IReadOnlyList<float> samples, CancellationToken ct = default)
    {
        var viewers = EveryViewerOf(patientId);
        var method = MonitoringNames.EcgEvent(patientId);

        foreach (var sample in samples)
        {
            if (!await TrySendAsync(viewers, method, sample, ct))
            {
                return;
            }
        }
    }

    /// <inheritdoc />
    public async Task NotifyRiskStatusAsync(RiskStatusBroadcast broadcast, CancellationToken ct = default)
    {
        var patientId = broadcast.Clinical.PatientId;
        var method = MonitoringNames.RiskStatusEvent(patientId);

        await TrySendAsync(hub.Clients.Group(MonitoringNames.PatientDoctorsGroup(patientId)), method,
            broadcast.Clinical, ct);
        await TrySendAsync(hub.Clients.Group(MonitoringNames.PatientGroup(patientId)), method,
            broadcast.PatientView, ct);
    }

    /// <inheritdoc />
    public Task NotifyAlertAsync(AlertDto alert, CancellationToken ct = default) =>
        TrySendAsync(EveryViewerOf(alert.PatientId), MonitoringNames.AlertEvent(alert.PatientId), alert, ct);

    /// <inheritdoc />
    public async Task NotifyPatientSummaryAsync(
        IReadOnlyCollection<Guid> doctorIds, PatientSummaryUpdate update, CancellationToken ct = default)
    {
        foreach (var doctorId in doctorIds)
        {
            await TrySendAsync(
                hub.Clients.Group(MonitoringNames.DoctorGroup(doctorId)),
                MonitoringNames.PatientSummaryEvent(doctorId),
                update,
                ct);
        }
    }

    /// <summary>The patient's own connections plus the doctors currently viewing the patient.</summary>
    private IClientProxy EveryViewerOf(Guid patientId) =>
        hub.Clients.Groups(MonitoringNames.PatientGroup(patientId), MonitoringNames.PatientDoctorsGroup(patientId));

    /// <summary>Sends one message; returns false (after logging) when the transport fails.</summary>
    private async Task<bool> TrySendAsync(IClientProxy audience, string method, object payload, CancellationToken ct)
    {
        try
        {
            await audience.SendAsync(method, payload, ct);
            return true;
        }
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
            logger.LogWarning(exception, "Real-time push {Method} failed.", method);
            return false;
        }
    }
}
