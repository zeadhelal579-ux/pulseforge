// =============================================================================
// File           : RegisterDoctorValidator.cs
// Layer          : Application / Auth/Commands/RegisterDoctor
// Purpose        : Validates the doctor signup request.
// Logic          : Same rules and length limits as doctorSignupSchema (Zod) in the frontend; limits come from
//                  the entity constants.
// Implementation : FluentValidation validator discovered automatically by AddValidatorsFromAssembly.
// =============================================================================
using CardioSense.Application.Common.Contracts;
using CardioSense.Application.Common.Validation;
using CardioSense.Domain.Entities;
using FluentValidation;

namespace CardioSense.Application.Auth.Commands.RegisterDoctor;

/// <summary>Rules for <see cref="RegisterDoctorCommand"/>.</summary>
internal sealed class RegisterDoctorValidator : AbstractValidator<RegisterDoctorCommand>
{
    /// <summary>Declares the rules.</summary>
    public RegisterDoctorValidator()
    {
        RuleFor(x => x.FullName).NotEmpty().MaximumLength(Doctor.NameMaxLength);
        RuleFor(x => x.Phone).NotEmpty().MaximumLength(Doctor.PhoneMaxLength);
        RuleFor(x => x.Email).ValidEmail();
        RuleFor(x => x.Password).ValidPassword();
        RuleFor(x => x.Specialty).NotEmpty().MaximumLength(Doctor.SpecialtyMaxLength);
        RuleFor(x => x.LicenseNumber).NotEmpty().MaximumLength(Doctor.LicenseNumberMaxLength);
        RuleFor(x => x.Hospital).NotEmpty().MaximumLength(Doctor.HospitalMaxLength);
        RuleFor(x => x.Department).MaximumLength(Doctor.DepartmentMaxLength);
        RuleFor(x => x.PreferredLanguage).Must(ContractLiterals.Languages.IsSupported)
            .WithMessage("Preferred language must be 'ar' or 'en'.");
    }
}
