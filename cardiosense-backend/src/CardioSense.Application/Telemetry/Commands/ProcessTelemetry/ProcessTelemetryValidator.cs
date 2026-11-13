// =============================================================================
// File           : ProcessTelemetryValidator.cs
// Layer          : Application / Telemetry/Commands/ProcessTelemetry
// Purpose        : Validates a telemetry message before it is processed.
// Logic          : Bounds come from the entities (single source of truth). A timestamp more than five minutes
//                  in the future is rejected because it signals a device clock problem that would corrupt the
//                  time series.
// Implementation : FluentValidation validator that receives TimeProvider so 'now' is testable.
// =============================================================================
using CardioSense.Domain.Entities;
using FluentValidation;

namespace CardioSense.Application.Telemetry.Commands.ProcessTelemetry;

/// <summary>Rules for <see cref="ProcessTelemetryCommand"/>.</summary>
internal sealed class ProcessTelemetryValidator : AbstractValidator<ProcessTelemetryCommand>
{
    /// <summary>Largest number of live waveform samples accepted in one message.</summary>
    private const int MaxEcgSamples = 5000;

    /// <summary>How far ahead of the server clock a device timestamp may be.</summary>
    private static readonly TimeSpan MaxClockSkew = TimeSpan.FromMinutes(5);

    /// <summary>Declares the rules.</summary>
    public ProcessTelemetryValidator(TimeProvider clock)
    {
        RuleFor(x => x.SerialNumber).NotEmpty().MaximumLength(Device.SerialNumberMaxLength);
        RuleFor(x => x.Timestamp)
            .LessThanOrEqualTo(_ => clock.GetUtcNow() + MaxClockSkew)
            .WithMessage("Timestamp must not be in the future.");
        RuleFor(x => x.HeartRate).InclusiveBetween(VitalsSample.MinHeartRate, VitalsSample.MaxHeartRate);
        RuleFor(x => x.Spo2).InclusiveBetween(VitalsSample.MinSpo2, VitalsSample.MaxSpo2);
        RuleFor(x => x.PrIntervalMs).InclusiveBetween(1, EcgFeatureSet.MaxIntervalMs);
        RuleFor(x => x.QrsDurationMs).InclusiveBetween(1, EcgFeatureSet.MaxIntervalMs);
        RuleFor(x => x.QtMs).InclusiveBetween(1, EcgFeatureSet.MaxIntervalMs);
        RuleFor(x => x.QtcMs).InclusiveBetween(1, EcgFeatureSet.MaxIntervalMs);
        RuleFor(x => x.FeatureVectorJson).MaximumLength(EcgFeatureSet.FeatureVectorMaxLength);
        RuleFor(x => x.EcgSamples)
            .Must(samples => samples is null || samples.Count <= MaxEcgSamples)
            .WithMessage($"A message may carry at most {MaxEcgSamples} waveform samples.");
    }
}
