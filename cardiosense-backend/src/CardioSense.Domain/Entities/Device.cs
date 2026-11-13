// =============================================================================
// File           : Device.cs
// Layer          : Domain / Entities
// Purpose        : A wearable device (ESP32 + sensors) identified by its serial number.
// Logic          : A device exists in inventory first and is paired to at most one patient (rule from the
//                  architecture doc); status messages refresh battery, signal and last-seen time.
// Implementation : Sealed entity; PairTo returns a Result because 'already paired' is an expected failure.
// =============================================================================
using CardioSense.Domain.Common;
using CardioSense.Domain.Enums;

namespace CardioSense.Domain.Entities;

/// <summary>A wearable monitoring device.</summary>
public sealed class Device : Entity
{
    /// <summary>Maximum length of the serial number.</summary>
    public const int SerialNumberMaxLength = 60;

    /// <summary>Maximum length of the firmware version text.</summary>
    public const int FirmwareVersionMaxLength = 30;

    /// <summary>Firmware shown until the device reports a real one.</summary>
    public const string UnknownFirmware = "unknown";

    /// <summary>Used by EF Core.</summary>
    private Device()
    {
    }

    /// <summary>Builds an inventory device that is not paired to anyone yet.</summary>
    private Device(string serialNumber, DateTimeOffset now)
        : base(now)
    {
        SerialNumber = RequireText(serialNumber, nameof(serialNumber), SerialNumberMaxLength);
    }

    /// <summary>Manufacturing serial number (unique).</summary>
    public string SerialNumber { get; private set; } = null!;

    /// <summary>The patient this device is paired to; null while it sits in inventory.</summary>
    public Guid? PatientId { get; private set; }

    /// <summary>When the device was paired.</summary>
    public DateTimeOffset? PairedAt { get; private set; }

    /// <summary>Last reported firmware version.</summary>
    public string FirmwareVersion { get; private set; } = UnknownFirmware;

    /// <summary>Last reported battery percentage (0-100).</summary>
    public int BatteryLevel { get; private set; }

    /// <summary>When the backend last heard from the device.</summary>
    public DateTimeOffset? LastSeenAt { get; private set; }

    /// <summary>Transport used by the last message.</summary>
    public ConnectionMode ConnectionMode { get; private set; }

    /// <summary>Last reported signal strength in dBm.</summary>
    public int? SignalStrengthDbm { get; private set; }

    /// <summary>Adds a device to the inventory.</summary>
    public static Device Register(string serialNumber, DateTimeOffset now) => new(serialNumber, now);

    /// <summary>Pairs the device to a patient; fails if it already belongs to another patient.</summary>
    public Result PairTo(Guid patientId, DateTimeOffset now)
    {
        if (PatientId is not null && PatientId != patientId)
        {
            return Result.Failure(DomainErrors.Device.AlreadyPaired);
        }

        PatientId = RequireId(patientId, nameof(patientId));
        PairedAt ??= now;
        return Result.Success();
    }

    /// <summary>Marks the device as seen; the timestamp only ever moves forward.</summary>
    public void RecordSeen(DateTimeOffset now)
    {
        if (LastSeenAt is null || now > LastSeenAt)
        {
            LastSeenAt = now;
        }
    }

    /// <summary>Applies a status message (battery, transport, firmware, signal) and marks the device seen.</summary>
    public void RecordStatus(
        int batteryLevel, ConnectionMode connectionMode, string? firmwareVersion, int? signalStrengthDbm,
        DateTimeOffset now)
    {
        BatteryLevel = RequireRange(batteryLevel, 0, 100, nameof(batteryLevel));
        ConnectionMode = connectionMode;
        SignalStrengthDbm = signalStrengthDbm;

        if (!string.IsNullOrWhiteSpace(firmwareVersion))
        {
            FirmwareVersion = RequireText(firmwareVersion, nameof(firmwareVersion), FirmwareVersionMaxLength);
        }

        RecordSeen(now);
    }

    /// <summary>True when the device was heard from inside the given window.</summary>
    public bool IsLive(DateTimeOffset now, TimeSpan window) => LastSeenAt is { } seen && now - seen <= window;
}
