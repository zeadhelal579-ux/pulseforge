// =============================================================================
// File           : GetPatientVitalsHistoryHandler.cs
// Layer          : Application / Patients/Queries/GetPatientVitalsHistory
// Purpose        : Handles GetPatientVitalsHistoryQuery.
// Logic          : Each range maps to a window and a bucket size so the chart always receives at most one
//                  real reading per bucket (24h/5min, 7d/1h, 30d/6h) instead of tens of thousands of raw
//                  points.
// Implementation : Internal sealed handler; the down-sampling itself is done by IVitalsTimeSeriesStore (in
//                  the database, not in memory).
// =============================================================================
using CardioSense.Application.Common.Contracts;
using CardioSense.Application.Common.Dtos;
using CardioSense.Application.Common.Mappings;
using CardioSense.Domain.Common;
using CardioSense.Domain.Interfaces;
using Mediator;

namespace CardioSense.Application.Patients.Queries.GetPatientVitalsHistory;

/// <summary>Handles <see cref="GetPatientVitalsHistoryQuery"/>.</summary>
internal sealed class GetPatientVitalsHistoryHandler(IVitalsTimeSeriesStore vitals, TimeProvider clock)
    : IRequestHandler<GetPatientVitalsHistoryQuery, Result<IReadOnlyList<VitalsSampleDto>>>
{
    /// <summary>Loads the down-sampled history for the requested range.</summary>
    public async ValueTask<Result<IReadOnlyList<VitalsSampleDto>>> Handle(
        GetPatientVitalsHistoryQuery request, CancellationToken ct)
    {
        if (!ContractLiterals.VitalsRanges.TryResolve(request.Range, out var window, out var bucket))
        {
            return Result.Failure<IReadOnlyList<VitalsSampleDto>>(
                Error.Validation("Vitals.InvalidRange", "Range must be '24h', '7d' or '30d'."));
        }

        var now = clock.GetUtcNow();
        var samples = await vitals.GetHistoryAsync(request.PatientId, now - window, now, bucket, ct);

        IReadOnlyList<VitalsSampleDto> history = samples.Select(sample => sample.ToDto()).ToList();
        return Result.Success(history);
    }
}
