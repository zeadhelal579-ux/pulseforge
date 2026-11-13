// =============================================================================
// File           : InMemoryVitalsStore.cs
// Layer          : Tests / Support
// Purpose        : IVitalsTimeSeriesStore backed by a list.
// Logic          : Handlers must work with any store behind the Domain interface (SQL today, a time-series database
//                  later); this list-based implementation proves they only depend on the interface. History is
//                  returned unbucketed - bucketing is a storage concern covered by the Infrastructure layer.
// Implementation : LINQ over a list of samples.
// =============================================================================
using CardioSense.Domain.Entities;
using CardioSense.Domain.Interfaces;

namespace CardioSense.Application.Tests.Support;

/// <summary>A list-backed vitals store.</summary>
public sealed class InMemoryVitalsStore : IVitalsTimeSeriesStore
{
    /// <summary>Every sample appended so far.</summary>
    public List<VitalsSample> Samples { get; } = [];

    /// <inheritdoc />
    public Task AppendAsync(VitalsSample sample, CancellationToken ct = default)
    {
        Samples.Add(sample);
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public Task<VitalsSample?> GetLatestAsync(Guid patientId, CancellationToken ct = default) =>
        Task.FromResult(Samples
            .Where(sample => sample.PatientId == patientId)
            .OrderByDescending(sample => sample.Timestamp)
            .FirstOrDefault());

    /// <inheritdoc />
    public Task<IReadOnlyDictionary<Guid, VitalsSample>> GetLatestForPatientsAsync(
        IReadOnlyCollection<Guid> patientIds, CancellationToken ct = default)
    {
        IReadOnlyDictionary<Guid, VitalsSample> latest = Samples
            .Where(sample => patientIds.Contains(sample.PatientId))
            .GroupBy(sample => sample.PatientId)
            .ToDictionary(group => group.Key, group => group.OrderByDescending(sample => sample.Timestamp).First());

        return Task.FromResult(latest);
    }

    /// <inheritdoc />
    public Task<IReadOnlyList<VitalsSample>> GetHistoryAsync(
        Guid patientId, DateTimeOffset from, DateTimeOffset to, TimeSpan bucketSize, CancellationToken ct = default)
    {
        IReadOnlyList<VitalsSample> history = Samples
            .Where(sample => sample.PatientId == patientId && sample.Timestamp >= from && sample.Timestamp <= to)
            .OrderBy(sample => sample.Timestamp)
            .ToList();

        return Task.FromResult(history);
    }

    /// <inheritdoc />
    public Task<IReadOnlyDictionary<Guid, HeartRateRange>> GetHeartRateRangesAsync(
        IReadOnlyCollection<Guid> patientIds, DateTimeOffset since, CancellationToken ct = default)
    {
        IReadOnlyDictionary<Guid, HeartRateRange> ranges = Samples
            .Where(sample => patientIds.Contains(sample.PatientId) && sample.Timestamp >= since)
            .GroupBy(sample => sample.PatientId)
            .ToDictionary(
                group => group.Key,
                group => new HeartRateRange(
                    group.Min(sample => sample.HeartRate), group.Max(sample => sample.HeartRate)));

        return Task.FromResult(ranges);
    }

    /// <inheritdoc />
    public Task<double?> GetAverageHeartRateAsync(
        Guid patientId, DateTimeOffset from, DateTimeOffset to, CancellationToken ct = default)
    {
        var rates = Samples
            .Where(sample => sample.PatientId == patientId && sample.Timestamp >= from && sample.Timestamp <= to)
            .Select(sample => (double)sample.HeartRate)
            .ToList();

        return Task.FromResult(rates.Count == 0 ? (double?)null : rates.Average());
    }
}
