// =============================================================================
// File           : EcgFeatureSet.cs
// Layer          : Domain / Entities
// Purpose        : Processed ECG features extracted on the ESP32 (PR, QRS, QT, QTc) - not the raw waveform.
// Logic          : Intervals must be positive durations; the raw feature vector is kept as JSON for future
//                  model retraining.
// Implementation : Sealed entity; Create returns a Result. Intervals are whole milliseconds.
// =============================================================================
using CardioSense.Domain.Common;

namespace CardioSense.Domain.Entities;

/// <summary>Features extracted from one ECG window.</summary>
public sealed class EcgFeatureSet : Entity
{
    /// <summary>Generous upper bound that only rejects absurd values (abnormal real values must still pass).</summary>
    public const int MaxIntervalMs = 2000;

    /// <summary>Maximum length of the stored raw feature JSON.</summary>
    public const int FeatureVectorMaxLength = 8000;

    /// <summary>Used by EF Core.</summary>
    private EcgFeatureSet()
    {
    }

    /// <summary>Builds a feature set from already validated values.</summary>
    private EcgFeatureSet(
        Guid patientId, DateTimeOffset timestamp, int prIntervalMs, int qrsDurationMs, int qtMs, int qtcMs,
        string? featureVectorJson)
        : base(timestamp)
    {
        PatientId = RequireId(patientId, nameof(patientId));
        Timestamp = timestamp;
        PrIntervalMs = prIntervalMs;
        QrsDurationMs = qrsDurationMs;
        QtMs = qtMs;
        QtcMs = qtcMs;
        FeatureVectorJson = RequireOptionalText(featureVectorJson, nameof(featureVectorJson), FeatureVectorMaxLength);
    }

    /// <summary>The patient the features belong to.</summary>
    public Guid PatientId { get; private set; }

    /// <summary>When the ECG window was captured.</summary>
    public DateTimeOffset Timestamp { get; private set; }

    /// <summary>PR interval in milliseconds.</summary>
    public int PrIntervalMs { get; private set; }

    /// <summary>QRS duration in milliseconds.</summary>
    public int QrsDurationMs { get; private set; }

    /// <summary>QT interval in milliseconds.</summary>
    public int QtMs { get; private set; }

    /// <summary>Corrected QT interval in milliseconds.</summary>
    public int QtcMs { get; private set; }

    /// <summary>Raw feature vector as sent by the device (JSON), kept for model retraining.</summary>
    public string? FeatureVectorJson { get; private set; }

    /// <summary>Creates a feature set, or fails when an interval is not a plausible positive duration.</summary>
    public static Result<EcgFeatureSet> Create(
        Guid patientId, DateTimeOffset timestamp, int prIntervalMs, int qrsDurationMs, int qtMs, int qtcMs,
        string? featureVectorJson)
    {
        if (!IsPlausible(prIntervalMs) || !IsPlausible(qrsDurationMs) || !IsPlausible(qtMs) || !IsPlausible(qtcMs))
        {
            return Result.Failure<EcgFeatureSet>(DomainErrors.Ecg.IntervalOutOfRange);
        }

        return Result.Success(
            new EcgFeatureSet(patientId, timestamp, prIntervalMs, qrsDurationMs, qtMs, qtcMs, featureVectorJson));
    }

    /// <summary>True when the interval is a positive duration below the sanity ceiling.</summary>
    private static bool IsPlausible(int milliseconds) => milliseconds is > 0 and <= MaxIntervalMs;
}
