// =============================================================================
// File           : IVitalsTimeSeriesStore.cs
// Layer          : Domain / Interfaces
// Purpose        : Storage contract for the high-frequency vitals time series (the Phase 1 -> Phase 2 seam).
// Logic          : Application handlers depend only on this interface for vitals reads/writes, so SQL Server
//                  can be swapped for InfluxDB/TimescaleDB later by changing one DI registration.
// Implementation : Interface only; SqlVitalsTimeSeriesStore (Phase 1) and InfluxVitalsTimeSeriesStore (Phase
//                  2) implement it in Infrastructure.
// =============================================================================
using CardioSense.Domain.Entities;

namespace CardioSense.Domain.Interfaces;

/// <summary>Reads and writes the vitals time series.</summary>
public interface IVitalsTimeSeriesStore
{
    /// <summary>Appends one reading.</summary>
    Task AppendAsync(VitalsSample sample, CancellationToken ct = default);

    /// <summary>Gets the most recent reading of a patient, or null when there is none.</summary>
    Task<VitalsSample?> GetLatestAsync(Guid patientId, CancellationToken ct = default);

    /// <summary>Gets the most recent reading of each given patient (patients without readings are absent).</summary>
    Task<IReadOnlyDictionary<Guid, VitalsSample>> GetLatestForPatientsAsync(
        IReadOnlyCollection<Guid> patientIds, CancellationToken ct = default);

    /// <summary>
    /// Gets a down-sampled history: at most one reading (the newest) per bucket, oldest first, so a 30-day
    /// chart never receives tens of thousands of points.
    /// </summary>
    Task<IReadOnlyList<VitalsSample>> GetHistoryAsync(
        Guid patientId, DateTimeOffset from, DateTimeOffset to, TimeSpan bucketSize, CancellationToken ct = default);

    /// <summary>Gets the min/max heart rate since the given time for each patient that has readings.</summary>
    Task<IReadOnlyDictionary<Guid, HeartRateRange>> GetHeartRateRangesAsync(
        IReadOnlyCollection<Guid> patientIds, DateTimeOffset since, CancellationToken ct = default);

    /// <summary>Gets the average heart rate of a period, or null when the period has no readings.</summary>
    Task<double?> GetAverageHeartRateAsync(
        Guid patientId, DateTimeOffset from, DateTimeOffset to, CancellationToken ct = default);
}
