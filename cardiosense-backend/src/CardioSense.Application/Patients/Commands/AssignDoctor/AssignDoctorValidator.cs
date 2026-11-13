// =============================================================================
// File           : AssignDoctorValidator.cs
// Layer          : Application / Patients/Commands/AssignDoctor
// Purpose        : Validates the add-patient request.
// Logic          : The identifier is required and capped so a huge value cannot reach the database.
// Implementation : FluentValidation validator.
// =============================================================================
using FluentValidation;

namespace CardioSense.Application.Patients.Commands.AssignDoctor;

/// <summary>Rules for <see cref="AssignDoctorCommand"/>.</summary>
internal sealed class AssignDoctorValidator : AbstractValidator<AssignDoctorCommand>
{
    /// <summary>Longest identifier accepted.</summary>
    private const int IdentifierMaxLength = 64;

    /// <summary>Declares the rules.</summary>
    public AssignDoctorValidator()
    {
        RuleFor(x => x.Identifier).NotEmpty().MaximumLength(IdentifierMaxLength);
    }
}
