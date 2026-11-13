// =============================================================================
// File           : ProcessDeviceStatusCommand.cs
// Layer          : Application / Telemetry/Commands/ProcessDeviceStatus
// Purpose        : One status message from a device: battery, transport, firmware and signal strength.
// Logic          : Sent by the MQTT ingestion worker for messages on devices/{serial}/status (the slower
//                  topic). It keeps the device card on the dashboards ('Medical File & Device Telemetry') up
//                  to date.
// Implementation : Immutable record; the server time is used as 'last seen' because a device clock cannot be
//                  trusted.
// =============================================================================
using CardioSense.Domain.Common;
using CardioSense.Domain.Enums;
using Mediator;

namespace CardioSense.Application.Telemetry.Commands.ProcessDeviceStatus;

/// <summary>Process one device status message.</summary>
public sealed record ProcessDeviceStatusCommand(
    string SerialNumber,
    int BatteryLevel,
    ConnectionMode ConnectionMode,
    string? FirmwareVersion,
    int? SignalStrengthDbm) : IRequest<Result>;
