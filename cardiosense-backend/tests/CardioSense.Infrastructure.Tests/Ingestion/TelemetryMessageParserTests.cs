// =============================================================================
// File           : TelemetryMessageParserTests.cs
// Layer          : Tests / Ingestion
// Purpose        : Verifies how raw MQTT messages become commands - the contract the firmware must follow.
// Logic          : The serial number is taken from the topic, never from the payload (a device cannot claim to be
//                  another device by editing its JSON); a missing field is reported by name instead of becoming 0
//                  (a zero heart rate would look like a real, alarming reading); numbers must be whole numbers.
// Implementation : The parser is plain code with no MQTT in it, so it is tested directly with strings.
// =============================================================================
using CardioSense.Application.Telemetry.Commands.ProcessDeviceStatus;
using CardioSense.Application.Telemetry.Commands.ProcessTelemetry;
using CardioSense.Domain.Common;
using CardioSense.Domain.Enums;
using CardioSense.Infrastructure.Ingestion;

namespace CardioSense.Infrastructure.Tests.Ingestion;

/// <summary>Tests for TelemetryMessageParser.</summary>
public sealed class TelemetryMessageParserTests
{
    /// <summary>A telemetry topic for the test device.</summary>
    private const string TelemetryTopic = "devices/CS-0001/telemetry";

    /// <summary>A status topic for the test device.</summary>
    private const string StatusTopic = "devices/CS-0001/status";

    /// <summary>A complete, valid telemetry payload.</summary>
    private const string ValidTelemetry = "{" + VitalFields + "}";

    /// <summary>The required telemetry fields, without the surrounding braces.</summary>
    private const string VitalFields =
        "\"heartRate\":74,\"spo2\":98,\"prIntervalMs\":152,\"qrsDurationMs\":88,\"qtMs\":396,\"qtcMs\":418";

    /// <summary>The time the message was received.</summary>
    private static readonly DateTimeOffset ReceivedAt = new(2026, 11, 1, 12, 0, 0, TimeSpan.Zero);

    /// <summary>A valid message becomes a telemetry command with every value mapped.</summary>
    [Fact]
    public void A_valid_telemetry_message_becomes_a_command()
    {
        var result = TelemetryMessageParser.Parse(TelemetryTopic, ValidTelemetry, ReceivedAt);

        Assert.True(result.IsSuccess);
        var command = Assert.IsType<ProcessTelemetryCommand>(result.Value);
        Assert.Equal("CS-0001", command.SerialNumber);
        Assert.Equal(74, command.HeartRate);
        Assert.Equal(98, command.Spo2);
        Assert.Equal(152, command.PrIntervalMs);
        Assert.Equal(88, command.QrsDurationMs);
        Assert.Equal(396, command.QtMs);
        Assert.Equal(418, command.QtcMs);
    }

    /// <summary>Without a device timestamp the time of receipt is used.</summary>
    [Fact]
    public void A_missing_timestamp_defaults_to_the_time_of_receipt()
    {
        var result = TelemetryMessageParser.Parse(TelemetryTopic, ValidTelemetry, ReceivedAt);

        Assert.Equal(ReceivedAt, Assert.IsType<ProcessTelemetryCommand>(result.Value).Timestamp);
    }

    /// <summary>A timestamp sent by the device is kept.</summary>
    [Fact]
    public void A_device_timestamp_is_kept()
    {
        const string payload = "{\"timestamp\":\"2026-11-01T11:59:30Z\"," + VitalFields + "}";

        var result = TelemetryMessageParser.Parse(TelemetryTopic, payload, ReceivedAt);

        var expected = new DateTimeOffset(2026, 11, 1, 11, 59, 30, TimeSpan.Zero);
        Assert.Equal(expected, Assert.IsType<ProcessTelemetryCommand>(result.Value).Timestamp);
    }

    /// <summary>A device cannot impersonate another by putting a different serial number in the payload.</summary>
    [Fact]
    public void The_serial_number_comes_from_the_topic_only()
    {
        const string payload = "{\"serialNumber\":\"OTHER\"," + VitalFields + "}";

        var result = TelemetryMessageParser.Parse(TelemetryTopic, payload, ReceivedAt);

        Assert.Equal("CS-0001", Assert.IsType<ProcessTelemetryCommand>(result.Value).SerialNumber);
    }

    /// <summary>The optional feature object and the ECG samples are carried through.</summary>
    [Fact]
    public void Optional_features_and_ecg_samples_are_carried_through()
    {
        const string payload = "{" + VitalFields + ",\"ecgSamples\":[0.5,0.25],\"features\":{\"rr\":810}}";

        var result = TelemetryMessageParser.Parse(TelemetryTopic, payload, ReceivedAt);

        var command = Assert.IsType<ProcessTelemetryCommand>(result.Value);
        Assert.Equal(new[] { 0.5f, 0.25f }, command.EcgSamples);
        Assert.Contains("\"rr\":810", command.FeatureVectorJson);
    }

    /// <summary>Missing required fields are listed by name, not silently turned into zero.</summary>
    [Fact]
    public void Missing_fields_are_reported_by_name()
    {
        var result = TelemetryMessageParser.Parse(TelemetryTopic, """{"heartRate":74}""", ReceivedAt);

        Assert.True(result.IsFailure);
        Assert.Equal("Ingestion.MissingFields", result.Error.Code);
        Assert.Contains("spo2", result.Error.Description);
        Assert.Contains("qtcMs", result.Error.Description);
        Assert.DoesNotContain("heartRate", result.Error.Description);
    }

    /// <summary>Text that is not JSON, and a JSON null, are rejected as invalid.</summary>
    [Theory]
    [InlineData("not json")]
    [InlineData("null")]
    public void A_payload_that_is_not_a_json_object_is_rejected(string payload)
    {
        var result = TelemetryMessageParser.Parse(TelemetryTopic, payload, ReceivedAt);

        Assert.Equal("Ingestion.InvalidJson", result.Error.Code);
    }

    /// <summary>The firmware contract uses whole numbers: a fractional heart rate is rejected.</summary>
    [Fact]
    public void A_fractional_heart_rate_is_rejected()
    {
        const string payload =
            """{"heartRate":74.5,"spo2":98,"prIntervalMs":152,"qrsDurationMs":88,"qtMs":396,"qtcMs":418}""";

        var result = TelemetryMessageParser.Parse(TelemetryTopic, payload, ReceivedAt);

        Assert.Equal("Ingestion.InvalidJson", result.Error.Code);
    }

    /// <summary>Topics outside devices/{serial}/telemetry|status are not device messages.</summary>
    [Theory]
    [InlineData("devices/CS-0001/other")]
    [InlineData("devices//telemetry")]
    [InlineData("other/CS-0001/telemetry")]
    [InlineData("devices/CS-0001")]
    [InlineData("devices/CS-0001/telemetry/extra")]
    public void An_unsupported_topic_is_rejected(string topic)
    {
        var result = TelemetryMessageParser.Parse(topic, ValidTelemetry, ReceivedAt);

        Assert.Equal(ErrorType.Validation, result.Error.Type);
        Assert.Equal("Ingestion.UnsupportedTopic", result.Error.Code);
    }

    /// <summary>A status message becomes a device-status command; the connection mode is case-insensitive.</summary>
    [Fact]
    public void A_valid_status_message_becomes_a_command()
    {
        const string payload =
            """{"batteryLevel":87,"connectionMode":"bluetooth","firmwareVersion":"1.2.0","signalStrengthDbm":-61}""";

        var result = TelemetryMessageParser.Parse(StatusTopic, payload, ReceivedAt);

        var command = Assert.IsType<ProcessDeviceStatusCommand>(result.Value);
        Assert.Equal("CS-0001", command.SerialNumber);
        Assert.Equal(87, command.BatteryLevel);
        Assert.Equal(ConnectionMode.Bluetooth, command.ConnectionMode);
        Assert.Equal("1.2.0", command.FirmwareVersion);
        Assert.Equal(-61, command.SignalStrengthDbm);
    }

    /// <summary>An unknown connection mode - including a bare number - is rejected.</summary>
    [Theory]
    [InlineData("Zigbee")]
    [InlineData("7")]
    public void An_unknown_connection_mode_is_rejected(string mode)
    {
        var payload = $$"""{"batteryLevel":87,"connectionMode":"{{mode}}"}""";

        var result = TelemetryMessageParser.Parse(StatusTopic, payload, ReceivedAt);

        Assert.Equal("Ingestion.MissingFields", result.Error.Code);
        Assert.Contains("connectionMode", result.Error.Description);
    }
}
