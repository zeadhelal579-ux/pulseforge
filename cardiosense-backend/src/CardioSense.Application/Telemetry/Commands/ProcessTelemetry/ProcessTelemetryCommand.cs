// =============================================================================
// File           : ProcessTelemetryCommand.cs
// Layer          : Application / Telemetry/Commands/ProcessTelemetry
// Purpose        : One telemetry message from a device: vitals, ECG features and (optionally) a window of
//                  live waveform samples.
// Logic          : Sent by the MQTT ingestion worker for every message on devices/{serial}/telemetry. The
//                  waveform samples are only relayed live to browsers and are never stored (raw ECG is not
//                  persisted by design).
// Implementation : Immutable record; the ingestion parser fills Timestamp with the receive time when the
//                  device sends none.
// =============================================================================
using CardioSense.Domain.Common;
using Mediator;

namespace CardioSense.Application.Telemetry.Commands.ProcessTelemetry;

/// <summary>Process one telemetry message.</summary>
public sealed record ProcessTelemetryCommand(
    string SerialNumber,
    DateTimeOffset Timestamp,
    int HeartRate,
    int Spo2,
    int PrIntervalMs,
    int QrsDurationMs,
    int QtMs,
    int QtcMs,
    string? FeatureVectorJson,
    IReadOnlyList<float>? EcgSamples) : IRequest<Result>;
