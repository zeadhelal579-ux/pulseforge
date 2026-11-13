// =============================================================================
// File           : DeviceSummaryDto.cs
// Layer          : Application / Common/Dtos
// Purpose        : Device card data: serial, firmware, battery, last seen, transport and signal.
// Logic          : Serves both the profile aggregate and the device endpoints (the frontend's
//                  DeviceStatusResponse is the same shape).
// Implementation : Immutable positional records; property names are the PascalCase twins of the TypeScript
//                  fields (camelCase on the wire).
// =============================================================================
using CardioSense.Domain.Enums;

namespace CardioSense.Application.Common.Dtos;

/// <summary>Summary of a paired device.</summary>
public sealed record DeviceSummaryDto(
    string SerialNumber,
    string FirmwareVersion,
    int BatteryLevel,
    DateTimeOffset LastSeenAt,
    ConnectionMode ConnectionMode,
    int? SignalStrengthDbm);
