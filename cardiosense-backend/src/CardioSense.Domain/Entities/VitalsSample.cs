// =============================================================================
// File           : VitalsSample.cs
// Layer          : Domain / Entities
// Purpose        : One heart-rate / SpO2 reading from a device (time-series row).
// Logic          : Readings outside the sensor's plausible window are rejected as artefacts instead of being
//                  stored.
// Implementation : Sealed entity; Create returns a Result. CreatedAt equals Timestamp because rows are
//                  append-only.
// =============================================================================
using CardioSense.Domain.Common;

namespace CardioSense.Domain.Entities;

/// <summary>A single vitals reading.</summary>
public sealed class VitalsSample : Entity
{
    /// <summary>Lowest accepted heart rate (bpm).</summary>
    public const int MinHeartRate = 20;

    /// <summary>Highest accepted heart rate (bpm).</summary>
    public const int MaxHeartRate = 300;

    /// <summary>Lowest accepted SpO2 (percent).</summary>
    public const int MinSpo2 = 50;

    /// <summary>Highest accepted SpO2 (percent).</summary>
    public const int MaxSpo2 = 100;

    /// <summary>Used by EF Core.</summary>
    private VitalsSample()
    {
    }

    /// <summary>Builds a sample from already validated values.</summary>
    private VitalsSample(Guid patientId, Guid deviceId, DateTimeOffset timestamp, int heartRate, int spo2)
        : base(timestamp)
    {
        PatientId = RequireId(patientId, nameof(patientId));
        DeviceId = RequireId(deviceId, nameof(deviceId));
        Timestamp = timestamp;
        HeartRate = heartRate;
        Spo2 = spo2;
    }

    /// <summary>The patient the reading belongs to.</summary>
    public Guid PatientId { get; private set; }

    /// <summary>The device that produced the reading.</summary>
    public Guid DeviceId { get; private set; }

    /// <summary>When the reading was taken.</summary>
    public DateTimeOffset Timestamp { get; private set; }

    /// <summary>Heart rate in beats per minute.</summary>
    public int HeartRate { get; private set; }

    /// <summary>Oxygen saturation in percent.</summary>
    public int Spo2 { get; private set; }

    /// <summary>Creates a sample, or fails when a value is not physiologically plausible.</summary>
    public static Result<VitalsSample> Create(
        Guid patientId, Guid deviceId, DateTimeOffset timestamp, int heartRate, int spo2)
    {
        if (heartRate is < MinHeartRate or > MaxHeartRate)
        {
            return Result.Failure<VitalsSample>(DomainErrors.Vitals.HeartRateOutOfRange);
        }

        if (spo2 is < MinSpo2 or > MaxSpo2)
        {
            return Result.Failure<VitalsSample>(DomainErrors.Vitals.Spo2OutOfRange);
        }

        return Result.Success(new VitalsSample(patientId, deviceId, timestamp, heartRate, spo2));
    }
}
