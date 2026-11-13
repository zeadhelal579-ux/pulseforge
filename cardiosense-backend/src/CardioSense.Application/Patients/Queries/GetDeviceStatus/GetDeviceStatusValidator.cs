// =============================================================================
// File           : GetDeviceStatusValidator.cs
// Layer          : Application / Patients/Queries/GetDeviceStatus
// Purpose        : Validates the serial number of the device status query.
// Logic          : A serial is required and capped at the length the Device entity allows.
// Implementation : FluentValidation validator using the entity constant.
// =============================================================================
using CardioSense.Domain.Entities;
using FluentValidation;

namespace CardioSense.Application.Patients.Queries.GetDeviceStatus;

/// <summary>Rules for <see cref="GetDeviceStatusQuery"/>.</summary>
internal sealed class GetDeviceStatusValidator : AbstractValidator<GetDeviceStatusQuery>
{
    /// <summary>Declares the rules.</summary>
    public GetDeviceStatusValidator()
    {
        RuleFor(x => x.SerialNumber).NotEmpty().MaximumLength(Device.SerialNumberMaxLength);
    }
}
