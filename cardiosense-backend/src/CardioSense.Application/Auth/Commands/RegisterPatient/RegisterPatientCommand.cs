// =============================================================================
// File           : RegisterPatientCommand.cs
// Layer          : Application / Auth/Commands/RegisterPatient
// Purpose        : Request to create a patient account and profile (the patient signup form).
// Logic          : Body of POST /api/auth/signup/patient - same fields as PatientSignupRequest in the
//                  frontend; the result is a ready session so the user lands on the dashboard without a
//                  second login step.
// Implementation : Immutable record; Age (not a date of birth) is what the form collects, so the handler
//                  derives an approximate date of birth.
// =============================================================================
using CardioSense.Application.Auth.Common;
using CardioSense.Domain.Common;
using Mediator;

namespace CardioSense.Application.Auth.Commands.RegisterPatient;

/// <summary>Register a patient.</summary>
public sealed record RegisterPatientCommand(
    string FullName,
    string Phone,
    string Email,
    string Password,
    int Age,
    string Gender,
    string NationalId,
    string Diagnosis,
    string? Medications,
    string? AssignedDoctorName,
    string? DeviceSerial,
    string PreferredLanguage) : IRequest<Result<AuthSession>>;
