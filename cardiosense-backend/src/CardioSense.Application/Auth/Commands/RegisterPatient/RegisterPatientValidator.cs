// =============================================================================
// File           : RegisterPatientValidator.cs
// Layer          : Application / Auth/Commands/RegisterPatient
// Purpose        : Validates the patient signup request.
// Logic          : Same rules and length limits as patientSignupSchema (Zod) in the frontend, so client and
//                  server agree; limits come from the entity constants (single source of truth).
// Implementation : FluentValidation validator discovered automatically by AddValidatorsFromAssembly.
// =============================================================================
using CardioSense.Application.Common.Contracts;
using CardioSense.Application.Common.Validation;
using CardioSense.Domain.Entities;
using FluentValidation;

namespace CardioSense.Application.Auth.Commands.RegisterPatient;

/// <summary>Rules for <see cref="RegisterPatientCommand"/>.</summary>
internal sealed class RegisterPatientValidator : AbstractValidator<RegisterPatientCommand>
{
    /// <summary>Oldest age accepted by the signup form.</summary>
    private const int MaxAge = 120;

    /// <summary>Declares the rules.</summary>
    public RegisterPatientValidator()
    {
        RuleFor(x => x.FullName).NotEmpty().MaximumLength(Patient.NameMaxLength);
        RuleFor(x => x.Phone).NotEmpty().MaximumLength(Patient.PhoneMaxLength);
        RuleFor(x => x.Email).ValidEmail();
        RuleFor(x => x.Password).ValidPassword();
        RuleFor(x => x.Age).InclusiveBetween(0, MaxAge);
        RuleFor(x => x.Gender).Must(ContractLiterals.Genders.IsSupported)
            .WithMessage("Gender must be 'male' or 'female'.");
        RuleFor(x => x.NationalId).ValidNationalId();
        RuleFor(x => x.Diagnosis).NotEmpty().MaximumLength(MedicalHistoryEntry.DescriptionMaxLength);
        RuleFor(x => x.Medications).MaximumLength(MedicalHistoryEntry.DescriptionMaxLength);
        RuleFor(x => x.AssignedDoctorName).MaximumLength(Doctor.NameMaxLength);
        RuleFor(x => x.DeviceSerial).MaximumLength(Device.SerialNumberMaxLength);
        RuleFor(x => x.PreferredLanguage).Must(ContractLiterals.Languages.IsSupported)
            .WithMessage("Preferred language must be 'ar' or 'en'.");
    }
}
