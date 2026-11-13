// =============================================================================
// File           : ProcessDeviceStatusHandler.cs
// Layer          : Application / Telemetry/Commands/ProcessDeviceStatus
// Purpose        : Handles a device status message by updating the device row.
// Logic          : Works for any inventory device (paired or not) because a freshly shipped device may report
//                  before it is paired; unknown serials are rejected so the broker cannot be used to create
//                  devices.
// Implementation : Internal sealed handler; the update rules live in Device.RecordStatus.
// =============================================================================
using CardioSense.Application.Common.Errors;
using CardioSense.Application.Common.Interfaces;
using CardioSense.Domain.Common;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace CardioSense.Application.Telemetry.Commands.ProcessDeviceStatus;

/// <summary>Handles <see cref="ProcessDeviceStatusCommand"/>.</summary>
internal sealed class ProcessDeviceStatusHandler(IAppDbContext db, TimeProvider clock)
    : IRequestHandler<ProcessDeviceStatusCommand, Result>
{
    /// <summary>Applies the status to the device.</summary>
    public async ValueTask<Result> Handle(ProcessDeviceStatusCommand request, CancellationToken ct)
    {
        var serial = request.SerialNumber.Trim();
        var device = await db.Devices.FirstOrDefaultAsync(d => d.SerialNumber == serial, ct);
        if (device is null)
        {
            return Result.Failure(ApplicationErrors.Telemetry.UnknownDevice);
        }

        device.RecordStatus(
            request.BatteryLevel,
            request.ConnectionMode,
            request.FirmwareVersion,
            request.SignalStrengthDbm,
            clock.GetUtcNow());

        await db.SaveChangesAsync(ct);
        return Result.Success();
    }
}
