// =============================================================================
// File           : ProcessDeviceStatusValidator.cs
// Layer          : Application / Telemetry/Commands/ProcessDeviceStatus
// Purpose        : Validates a device status message.
// Logic          : Battery is a percentage, the transport must be a defined value and the signal strength a
//                  plausible negative dBm.
// Implementation : FluentValidation validator using the entity constants.
// =============================================================================
using CardioSense.Domain.Entities;
using FluentValidation;

namespace CardioSense.Application.Telemetry.Commands.ProcessDeviceStatus;

/// <summary>Rules for <see cref="ProcessDeviceStatusCommand"/>.</summary>
internal sealed class ProcessDeviceStatusValidator : AbstractValidator<ProcessDeviceStatusCommand>
{
    /// <summary>Weakest plausible signal strength in dBm.</summary>
    private const int MinSignalDbm = -150;

    /// <summary>Declares the rules.</summary>
    public ProcessDeviceStatusValidator()
    {
        RuleFor(x => x.SerialNumber).NotEmpty().MaximumLength(Device.SerialNumberMaxLength);
        RuleFor(x => x.BatteryLevel).InclusiveBetween(0, 100);
        RuleFor(x => x.ConnectionMode).IsInEnum();
        RuleFor(x => x.FirmwareVersion).MaximumLength(Device.FirmwareVersionMaxLength);
        RuleFor(x => x.SignalStrengthDbm).InclusiveBetween(MinSignalDbm, 0);
    }
}
