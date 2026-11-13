// =============================================================================
// File           : PatientListItemDto.cs
// Layer          : Application / Common/Dtos
// Purpose        : One row of the doctor's patient list.
// Logic          : Bed/room, blood pressure and ST elevation of the TypeScript type are optional there and
//                  not produced in Phase 1 (no data source), so they are omitted here.
// Implementation : Immutable positional records; property names are the PascalCase twins of the TypeScript
//                  fields (camelCase on the wire). HeartRateRangeDto is the 24-hour min/max pair.
// =============================================================================
using CardioSense.Domain.Enums;

namespace CardioSense.Application.Common.Dtos;

/// <summary>A patient row of the doctor's list.</summary>
public sealed record PatientListItemDto(
    Guid Id,
    string FullName,
    int Age,
    string Diagnosis,
    string FileNumber,
    RiskStatusLevel StatusLevel,
    string StatusLabel,
    string? AlertKind,
    bool IsLive,
    int HeartRate,
    HeartRateRangeDto HeartRateRange,
    int Spo2,
    int? RiskScore,
    DateTimeOffset LastUpdatedAt);

/// <summary>Minimum and maximum heart rate of the last 24 hours.</summary>
public sealed record HeartRateRangeDto(int Min, int Max);
