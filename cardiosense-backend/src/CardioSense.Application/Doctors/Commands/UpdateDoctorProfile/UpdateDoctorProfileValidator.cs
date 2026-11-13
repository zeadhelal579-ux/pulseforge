// =============================================================================
// File           : UpdateDoctorProfileValidator.cs
// Layer          : Application / Doctors/Commands/UpdateDoctorProfile
// Purpose        : Validates the partial doctor profile update.
// Logic          : A field is validated only when it is present; present values follow the same limits as
//                  signup.
// Implementation : FluentValidation validator using conditional rules, entity constants and the shared
//                  contract literals.
// =============================================================================
using CardioSense.Application.Common.Contracts;
using CardioSense.Domain.Entities;
using FluentValidation;

namespace CardioSense.Application.Doctors.Commands.UpdateDoctorProfile;

/// <summary>Rules for <see cref="UpdateDoctorProfileCommand"/>.</summary>
internal sealed class UpdateDoctorProfileValidator : AbstractValidator<UpdateDoctorProfileCommand>
{
    /// <summary>Declares the rules.</summary>
    public UpdateDoctorProfileValidator()
    {
        RuleFor(x => x.FullName).NotEmpty().MaximumLength(Doctor.NameMaxLength).When(x => x.FullName is not null);
        RuleFor(x => x.Phone).NotEmpty().MaximumLength(Doctor.PhoneMaxLength).When(x => x.Phone is not null);
        RuleFor(x => x.Specialty)
            .NotEmpty()
            .MaximumLength(Doctor.SpecialtyMaxLength)
            .When(x => x.Specialty is not null);
        RuleFor(x => x.Hospital).NotEmpty().MaximumLength(Doctor.HospitalMaxLength).When(x => x.Hospital is not null);
        RuleFor(x => x.Department).MaximumLength(Doctor.DepartmentMaxLength);
        RuleFor(x => x.PreferredLanguage)
            .Must(ContractLiterals.Languages.IsSupported)
            .WithMessage("Preferred language must be 'ar' or 'en'.")
            .When(x => x.PreferredLanguage is not null);
    }
}
