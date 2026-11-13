// =============================================================================
// File           : PairDeviceValidator.cs
// Layer          : Application / Patients/Commands/PairDevice
// Purpose        : Validates the pair-device request.
// Logic          : A serial number is required and capped at the length the Device entity allows.
// Implementation : FluentValidation validator using the entity constant.
// =============================================================================
using CardioSense.Domain.Entities;
using FluentValidation;

namespace CardioSense.Application.Patients.Commands.PairDevice;

/// <summary>Rules for <see cref="PairDeviceCommand"/>.</summary>
internal sealed class PairDeviceValidator : AbstractValidator<PairDeviceCommand>
{
    /// <summary>Declares the rules.</summary>
    public PairDeviceValidator()
    {
        RuleFor(x => x.SerialNumber).NotEmpty().MaximumLength(Device.SerialNumberMaxLength);
    }
}
