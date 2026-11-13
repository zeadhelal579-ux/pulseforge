// =============================================================================
// File           : HeartRateRange.cs
// Layer          : Domain / Interfaces
// Purpose        : Lowest and highest heart rate seen in a period.
// Logic          : Returned by the time-series store so the doctor's patient list can show a daily min/max.
// Implementation : Readonly record struct - a tiny immutable value type.
// =============================================================================
namespace CardioSense.Domain.Interfaces;

/// <summary>Minimum and maximum heart rate (bpm) of a period.</summary>
public readonly record struct HeartRateRange(int Min, int Max);
