// =============================================================================
// File           : GetDoctorProfileQuery.cs
// Layer          : Application / Doctors/Queries/GetDoctorProfile
// Purpose        : Query for the signed-in doctor's own profile.
// Logic          : Serves GET /api/doctors/me, which the frontend calls right after a silent refresh to learn
//                  the doctor's name and specialty.
// Implementation : Parameterless record implementing the Mediator request contract.
// =============================================================================
using CardioSense.Application.Common.Dtos;
using CardioSense.Domain.Common;
using Mediator;

namespace CardioSense.Application.Doctors.Queries.GetDoctorProfile;

/// <summary>Get the signed-in doctor's profile.</summary>
public sealed record GetDoctorProfileQuery : IRequest<Result<DoctorProfileDto>>;
