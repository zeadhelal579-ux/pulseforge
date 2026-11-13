// =============================================================================
// File           : ConnectionMode.cs
// Layer          : Domain / Enums
// Purpose        : How a device reaches the backend.
// Logic          : WiFi publishes straight to the MQTT broker; Bluetooth goes through the mobile app gateway.
// Implementation : Explicit numeric values so persisted data never shifts if members are reordered.
// =============================================================================
namespace CardioSense.Domain.Enums;

/// <summary>How a device reaches the backend.</summary>
public enum ConnectionMode
{
    /// <summary>Direct WiFi connection.</summary>
    WiFi = 0,

    /// <summary>BLE relayed by the mobile app.</summary>
    Bluetooth = 1
}
