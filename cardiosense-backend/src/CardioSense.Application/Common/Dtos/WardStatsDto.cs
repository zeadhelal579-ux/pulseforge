// =============================================================================
// File           : WardStatsDto.cs
// Layer          : Application / Common/Dtos
// Purpose        : Ward-level numbers of the doctor dashboard (active patients, critical cases, devices,
//                  signal quality, status counts).
// Logic          : Counts cover all of the doctor's assigned patients regardless of the list's
//                  search/filter/page.
// Implementation : Immutable positional records; property names are the PascalCase twins of the TypeScript
//                  fields (camelCase on the wire).
// =============================================================================
namespace CardioSense.Application.Common.Dtos;

/// <summary>Aggregate statistics for one doctor's caseload.</summary>
public sealed record WardStatsDto(
    int ActivePatients,
    int WardCapacity,
    int CriticalCasesCount,
    int CriticalCapacityTotal,
    int ConnectedDevices,
    int TotalDevices,
    double DeviceLinkRatePercent,
    double SignalAccuracyPercent,
    int StableCount,
    int CautionCount);
