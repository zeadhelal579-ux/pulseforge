// =============================================================================
// File           : DeviceMappings.cs
// Layer          : Application / Common/Mappings
// Purpose        : Maps a Device entity to DeviceSummaryDto.
// Logic          : A device that has never reported falls back to the pairing (then creation) time for
//                  LastSeenAt because the contract's timestamp is not optional.
// Implementation : Static extension methods; pure functions with no I/O.
// =============================================================================
using CardioSense.Application.Common.Dtos;
using CardioSense.Domain.Entities;

namespace CardioSense.Application.Common.Mappings;

/// <summary>Device mappings.</summary>
public static class DeviceMappings
{
    /// <summary>Maps a device to its summary DTO.</summary>
    public static DeviceSummaryDto ToDto(this Device device) => new(
        device.SerialNumber,
        device.FirmwareVersion,
        device.BatteryLevel,
        device.LastSeenAt ?? device.PairedAt ?? device.CreatedAt,
        device.ConnectionMode,
        device.SignalStrengthDbm);
}
