// =============================================================================
// File           : UpdatePatientProfileCommand.cs
// Layer          : Application / Patients/Commands/UpdatePatientProfile
// Purpose        : Request for a patient to update their own profile (partial update).
// Logic          : Body of PUT /api/patients/me: only the fields that are present change. Medical history is
//                  read-only here - clinical data should change through a doctor, not a free-text field.
// Implementation : Immutable record; null means 'leave unchanged'. The Api fills PatientId from the caller's
//                  claims.
// =============================================================================
using CardioSense.Application.Common.Dtos;
using CardioSense.Application.Common.Interfaces;
using CardioSense.Domain.Common;
using Mediator;

namespace CardioSense.Application.Patients.Commands.UpdatePatientProfile;

/// <summary>Update a patient profile.</summary>
public sealed record UpdatePatientProfileCommand(Guid PatientId, string? FullName, string? Phone,
    string? PreferredLanguage)
    : IRequest<Result<PatientProfileDto>>, IPatientScopedRequest;
