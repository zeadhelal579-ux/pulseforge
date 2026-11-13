// =============================================================================
// File           : UpdateDoctorProfileCommand.cs
// Layer          : Application / Doctors/Commands/UpdateDoctorProfile
// Purpose        : Request for a doctor to update their own profile (partial update).
// Logic          : Body of PUT /api/doctors/me: only the fields that are present change; the license number
//                  is never editable.
// Implementation : Immutable record; null means 'leave unchanged'.
// =============================================================================
using CardioSense.Application.Common.Dtos;
using CardioSense.Domain.Common;
using Mediator;

namespace CardioSense.Application.Doctors.Commands.UpdateDoctorProfile;

/// <summary>Update the signed-in doctor's profile.</summary>
public sealed record UpdateDoctorProfileCommand(
    string? FullName,
    string? Phone,
    string? Specialty,
    string? Hospital,
    string? Department,
    string? PreferredLanguage) : IRequest<Result<DoctorProfileDto>>;
