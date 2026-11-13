// =============================================================================
// File           : UpdatePatientProfileValidator.cs
// Layer          : Application / Patients/Commands/UpdatePatientProfile
// Purpose        : Validates the partial profile update.
// Logic          : A field is validated only when it is present; present values follow the same limits as
//                  signup.
// Implementation : FluentValidation validator using conditional rules and the shared contract literals.
// =============================================================================
using CardioSense.Application.Common.Contracts;
using CardioSense.Domain.Entities;
using FluentValidation;

namespace CardioSense.Application.Patients.Commands.UpdatePatientProfile;

/// <summary>Rules for <see cref="UpdatePatientProfileCommand"/>.</summary>
internal sealed class UpdatePatientProfileValidator : AbstractValidator<UpdatePatientProfileCommand>
{
    /// <summary>Declares the rules.</summary>
    public UpdatePatientProfileValidator()
    {
        RuleFor(x => x.FullName).NotEmpty().MaximumLength(Patient.NameMaxLength).When(x => x.FullName is not null);
        RuleFor(x => x.Phone).NotEmpty().MaximumLength(Patient.PhoneMaxLength).When(x => x.Phone is not null);
        RuleFor(x => x.PreferredLanguage)
            .Must(ContractLiterals.Languages.IsSupported)
            .WithMessage("Preferred language must be 'ar' or 'en'.")
            .When(x => x.PreferredLanguage is not null);
    }
}
