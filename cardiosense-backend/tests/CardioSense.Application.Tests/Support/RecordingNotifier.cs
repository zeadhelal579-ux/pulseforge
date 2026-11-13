// =============================================================================
// File           : RecordingNotifier.cs
// Layer          : Tests / Support
// Purpose        : IRealtimeNotifier that records what would have been pushed to browsers.
// Logic          : Tests assert on the pushes - especially that a patient never receives the clinical risk numbers -
//                  without any SignalR.
// Implementation : One public list per event type.
// =============================================================================
using CardioSense.Application.Common.Dtos;
using CardioSense.Application.Common.Interfaces;

namespace CardioSense.Application.Tests.Support;

/// <summary>Collects every real-time push.</summary>
public sealed class RecordingNotifier : IRealtimeNotifier
{
    /// <summary>Vitals ticks pushed.</summary>
    public List<VitalsTickPayload> Vitals { get; } = [];

    /// <summary>ECG samples pushed.</summary>
    public List<float> EcgSamples { get; } = [];

    /// <summary>Risk-status broadcasts pushed.</summary>
    public List<RiskStatusBroadcast> RiskStatuses { get; } = [];

    /// <summary>Alerts pushed.</summary>
    public List<AlertDto> Alerts { get; } = [];

    /// <summary>Patient-summary updates pushed to doctors.</summary>
    public List<PatientSummaryUpdate> Summaries { get; } = [];

    /// <inheritdoc />
    public Task NotifyVitalsAsync(VitalsTickPayload payload, CancellationToken ct = default)
    {
        Vitals.Add(payload);
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public Task NotifyEcgSamplesAsync(Guid patientId, IReadOnlyList<float> samples, CancellationToken ct = default)
    {
        EcgSamples.AddRange(samples);
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public Task NotifyRiskStatusAsync(RiskStatusBroadcast broadcast, CancellationToken ct = default)
    {
        RiskStatuses.Add(broadcast);
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public Task NotifyAlertAsync(AlertDto alert, CancellationToken ct = default)
    {
        Alerts.Add(alert);
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public Task NotifyPatientSummaryAsync(
        IReadOnlyCollection<Guid> doctorIds, PatientSummaryUpdate update, CancellationToken ct = default)
    {
        Summaries.Add(update);
        return Task.CompletedTask;
    }
}
