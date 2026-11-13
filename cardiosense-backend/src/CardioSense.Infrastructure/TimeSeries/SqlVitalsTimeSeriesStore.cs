// =============================================================================
// File           : SqlVitalsTimeSeriesStore.cs
// Layer          : Infrastructure / TimeSeries
// Purpose        : Phase 1 implementation of IVitalsTimeSeriesStore - the vitals time series lives in SQL Server.
// Logic          : Handlers depend only on the interface, so replacing this class with an InfluxDB/TimescaleDB store
//                  later is one DI registration. All aggregation (latest per patient, one point per chart bucket,
//                  min/max, average) runs inside the database; only the final few rows travel to the application.
// Implementation : Scoped service sharing the request's AppDbContext. History is read in two light queries (newest
//                  timestamp per bucket, then those rows) so a 30-day chart never loads tens of thousands of rows.
// =============================================================================
using CardioSense.Domain.Entities;
using CardioSense.Domain.Interfaces;
using CardioSense.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CardioSense.Infrastructure.TimeSeries;

/// <summary>Vitals time-series store backed by the VitalsSamples table.</summary>
internal sealed class SqlVitalsTimeSeriesStore(AppDbContext db) : IVitalsTimeSeriesStore
{
    /// <summary>
    /// Persists the reading before the task completes (it also commits any other pending changes of the current
    /// unit of work, which is what the telemetry handler wants anyway).
    /// </summary>
    public async Task AppendAsync(VitalsSample sample, CancellationToken ct = default)
    {
        db.VitalsSamples.Add(sample);
        await db.SaveChangesAsync(ct);
    }

    /// <inheritdoc />
    public Task<VitalsSample?> GetLatestAsync(Guid patientId, CancellationToken ct = default) =>
        db.VitalsSamples.AsNoTracking()
            .Where(sample => sample.PatientId == patientId)
            .OrderByDescending(sample => sample.Timestamp)
            .FirstOrDefaultAsync(ct);

    /// <inheritdoc />
    public async Task<IReadOnlyDictionary<Guid, VitalsSample>> GetLatestForPatientsAsync(
        IReadOnlyCollection<Guid> patientIds, CancellationToken ct = default)
    {
        if (patientIds.Count == 0)
        {
            return new Dictionary<Guid, VitalsSample>();
        }

        var newest = db.VitalsSamples.AsNoTracking()
            .Where(sample => patientIds.Contains(sample.PatientId))
            .GroupBy(sample => sample.PatientId)
            .Select(group => new { PatientId = group.Key, Timestamp = group.Max(sample => sample.Timestamp) });

        var rows = await (
            from sample in db.VitalsSamples.AsNoTracking()
            join latest in newest
                on new { sample.PatientId, sample.Timestamp } equals new { latest.PatientId, latest.Timestamp }
            select sample).ToListAsync(ct);

        // Two rows with the exact same timestamp would clash as keys, so keep the first per patient.
        return rows.GroupBy(sample => sample.PatientId).ToDictionary(group => group.Key, group => group.First());
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<VitalsSample>> GetHistoryAsync(
        Guid patientId, DateTimeOffset from, DateTimeOffset to, TimeSpan bucketSize, CancellationToken ct = default)
    {
        var bucketSeconds = Math.Max(1, (int)bucketSize.TotalSeconds);
        var window = db.VitalsSamples.AsNoTracking()
            .Where(sample => sample.PatientId == patientId && sample.Timestamp >= from && sample.Timestamp <= to);

        // Step 1 - the database finds the newest timestamp of every bucket (at most a few hundred values).
        var newestTimes = await window
            .GroupBy(sample => EF.Functions.DateDiffSecond(from, sample.Timestamp) / bucketSeconds)
            .Select(group => group.Max(sample => sample.Timestamp))
            .ToListAsync(ct);

        if (newestTimes.Count == 0)
        {
            return [];
        }

        // Step 2 - load exactly those rows, oldest first.
        var rows = await window
            .Where(sample => newestTimes.Contains(sample.Timestamp))
            .OrderBy(sample => sample.Timestamp)
            .ToListAsync(ct);

        // Guard: identical timestamps inside one window would otherwise yield two points for one bucket.
        return rows.GroupBy(sample => sample.Timestamp).Select(group => group.First()).ToList();
    }

    /// <inheritdoc />
    public async Task<IReadOnlyDictionary<Guid, HeartRateRange>> GetHeartRateRangesAsync(
        IReadOnlyCollection<Guid> patientIds, DateTimeOffset since, CancellationToken ct = default)
    {
        if (patientIds.Count == 0)
        {
            return new Dictionary<Guid, HeartRateRange>();
        }

        var ranges = await db.VitalsSamples.AsNoTracking()
            .Where(sample => patientIds.Contains(sample.PatientId) && sample.Timestamp >= since)
            .GroupBy(sample => sample.PatientId)
            .Select(group => new
            {
                PatientId = group.Key,
                Min = group.Min(sample => sample.HeartRate),
                Max = group.Max(sample => sample.HeartRate)
            })
            .ToListAsync(ct);

        return ranges.ToDictionary(range => range.PatientId, range => new HeartRateRange(range.Min, range.Max));
    }

    /// <inheritdoc />
    public Task<double?> GetAverageHeartRateAsync(
        Guid patientId, DateTimeOffset from, DateTimeOffset to, CancellationToken ct = default) =>
        db.VitalsSamples.AsNoTracking()
            .Where(sample => sample.PatientId == patientId && sample.Timestamp >= from && sample.Timestamp <= to)
            .Select(sample => (int?)sample.HeartRate)
            .AverageAsync(ct);
}
