// =============================================================================
// File           : DoctorProfileDto.cs
// Layer          : Application / Common/Dtos
// Purpose        : Doctor profile returned by GET/PUT /doctors/me.
// Logic          : Combines the Doctors row with the account's e-mail and preferred language.
// Implementation : Immutable positional records; property names are the PascalCase twins of the TypeScript
//                  fields (camelCase on the wire).
// =============================================================================
namespace CardioSense.Application.Common.Dtos;

/// <summary>A doctor's profile.</summary>
public sealed record DoctorProfileDto(
    Guid Id,
    string FullName,
    string Specialty,
    string LicenseNumber,
    string Hospital,
    string? Department,
    string Phone,
    string Email,
    string PreferredLanguage);
