// =============================================================================
// File           : PatientProfileDto.cs
// Layer          : Application / Common/Dtos
// Purpose        : The full patient aggregate returned by GET /patients/me and GET /patients/{id}.
// Logic          : One call returns profile + latest vitals + latest AI assessment + latest electrode check +
//                  medical history + the 24-hour status timeline, exactly the shape of PatientProfile in
//                  shared/types/api.ts.
// Implementation : Immutable positional records; property names are the PascalCase twins of the TypeScript
//                  fields (camelCase on the wire). Child records (doctor, history entry, timeline bucket)
//                  live here because they are used only by this aggregate.
// =============================================================================
using CardioSense.Domain.Enums;

namespace CardioSense.Application.Common.Dtos;

/// <summary>Patient profile plus the latest clinical snapshot.</summary>
public sealed record PatientProfileDto(
    Guid Id,
    string FullName,
    int Age,
    string Gender,
    string NationalId,
    string Phone,
    string Email,
    string Diagnosis,
    string? Medications,
    AssignedDoctorDto? AssignedDoctor,
    DeviceSummaryDto? Device,
    string PreferredLanguage,
    IReadOnlyList<MedicalHistoryEntryDto> MedicalHistory,
    VitalsSampleDto? LatestVitals,
    EcgFeatureSetDto? LatestEcgFeatureSet,
    RiskAssessmentDto? LatestRiskAssessment,
    ElectrodeVerificationDto? LatestElectrodeVerification,
    IReadOnlyList<HourlyStatusBucketDto> HourlyStatusTimeline);

/// <summary>The doctor the patient's call button rings.</summary>
public sealed record AssignedDoctorDto(Guid Id, string FullName, string Specialty, string Phone);

/// <summary>One medical history row.</summary>
public sealed record MedicalHistoryEntryDto(
    Guid Id, MedicalHistoryEntryType EntryType, string Description, DateTimeOffset RecordedAt);

/// <summary>Worst status inside one clock hour of the last 24 hours.</summary>
public sealed record HourlyStatusBucketDto(int Hour, RiskStatusLevel Status, bool IsCurrent);
