// =============================================================================
// File           : MqttOptions.cs
// Layer          : Infrastructure / Ingestion
// Purpose        : Settings of the connection to the MQTT broker that devices publish to.
// Logic          : TLS is on by default (port 8883) because medical readings must never cross a plain-text hop.
//                  Enabled lets tests or machines without a broker run the API without the ingestion worker.
// Implementation : Bound from the "Mqtt" configuration section and validated at start-up; credentials come from User
//                  Secrets or the environment, never from appsettings.json.
// =============================================================================
using System.ComponentModel.DataAnnotations;

namespace CardioSense.Infrastructure.Ingestion;

/// <summary>Settings of the MQTT ingestion worker.</summary>
public sealed class MqttOptions
{
    /// <summary>Configuration section name.</summary>
    public const string SectionName = "Mqtt";

    /// <summary>When false the ingestion worker does nothing.</summary>
    public bool Enabled { get; set; } = true;

    /// <summary>Broker host name.</summary>
    [Required]
    public string BrokerHost { get; set; } = "localhost";

    /// <summary>Broker TCP port (8883 is the standard MQTT-over-TLS port).</summary>
    [Range(1, 65535)]
    public int BrokerPort { get; set; } = 8883;

    /// <summary>Whether to encrypt the connection with TLS.</summary>
    public bool UseTls { get; set; } = true;

    /// <summary>Broker user name; empty for anonymous brokers.</summary>
    public string? Username { get; set; }

    /// <summary>Broker password.</summary>
    public string? Password { get; set; }

    /// <summary>MQTT client id (must be unique per running instance).</summary>
    [Required]
    public string ClientId { get; set; } = "cardiosense-ingestion";

    /// <summary>Seconds to wait before trying to reconnect after the connection is lost.</summary>
    [Range(1, 300)]
    public int ReconnectDelaySeconds { get; set; } = 5;
}
