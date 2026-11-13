// =============================================================================
// File           : RegisterDoctorCommand.cs
// Layer          : Application / Auth/Commands/RegisterDoctor
// Purpose        : Request to create a doctor account and profile (the doctor signup form).
// Logic          : Body of POST /api/auth/signup/doctor - same fields as DoctorSignupRequest in the frontend.
// Implementation : Immutable record implementing the Mediator request contract.
// =============================================================================
using CardioSense.Application.Auth.Common;
using CardioSense.Domain.Common;
using Mediator;

namespace CardioSense.Application.Auth.Commands.RegisterDoctor;

/// <summary>Register a doctor.</summary>
public sealed record RegisterDoctorCommand(
    string FullName,
    string Phone,
    string Email,
    string Password,
    string Specialty,
    string LicenseNumber,
    string Hospital,
    string? Department,
    string PreferredLanguage) : IRequest<Result<AuthSession>>;
