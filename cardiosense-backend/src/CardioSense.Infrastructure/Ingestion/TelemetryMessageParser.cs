// =============================================================================
// File           : TelemetryMessageParser.cs
// Layer          : Infrastructure / Ingestion
// Purpose        : Turns a raw MQTT message (topic + JSON text) into the matching application command.
// Logic          : Topics: devices/{serial}/telemetry -> ProcessTelemetryCommand, devices/{serial}/status ->
//                  ProcessDeviceStatusCommand. The serial comes from the topic (never from the payload) so a device
//                  cannot claim to be another one by editing its JSON. Missing required numbers are reported, never
//                  defaulted to 0. When a telemetry message has no timestamp the receive time is used.
//                  Telemetry JSON : { timestamp?, heartRate, spo2, prIntervalMs, qrsDurationMs, qtMs, qtcMs,
//                                     ecgSamples?: number[], features?: object }
//                  Status JSON    : { batteryLevel, connectionMode: "WiFi" | "Bluetooth", firmwareVersion?,
//                                     signalStrengthDbm? }
// Implementation : Pure static code with no MQTT types, so it is trivially unit-testable. Returns the command as
//                  IRequest<Result>, which both commands implement, so the worker needs no per-type branching.
// =============================================================================
using System.Text.Json;
using CardioSense.Application.Telemetry.Commands.ProcessDeviceStatus;
using CardioSense.Application.Telemetry.Commands.ProcessTelemetry;
using CardioSense.Domain.Common;
using CardioSense.Domain.Enums;
using Mediator;

namespace CardioSense.Infrastructure.Ingestion;

/// <summary>Parses device messages into application commands.</summary>
internal static class TelemetryMessageParser
{
    /// <summary>Subscription filter of every device's telemetry topic.</summary>
    public const string TelemetryTopicFilter = "devices/+/telemetry";

    /// <summary>Subscription filter of every device's status topic.</summary>
    public const string StatusTopicFilter = "devices/+/status";

    /// <summary>First segment of every device topic.</summary>
    private const string TopicRoot = "devices";

    /// <summary>Last segment of a telemetry topic.</summary>
    private const string TelemetrySegment = "telemetry";

    /// <summary>Last segment of a status topic.</summary>
    private const string StatusSegment = "status";

    /// <summary>Web defaults: camelCase property names, case-insensitive matching.</summary>
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    /// <summary>Parses one message; fails with a Validation error when it cannot be understood.</summary>
    public static Result<IRequest<Result>> Parse(string topic, string payload, DateTimeOffset receivedAt)
    {
        if (topic.Split('/') is not [TopicRoot, { Length: > 0 } serial, var kind])
        {
            return Result.Failure<IRequest<Result>>(IngestionErrors.UnsupportedTopic);
        }

        return kind switch
        {
            TelemetrySegment => ParseTelemetry(serial.Trim(), payload, receivedAt),
            StatusSegment => ParseStatus(serial.Trim(), payload),
            _ => Result.Failure<IRequest<Result>>(IngestionErrors.UnsupportedTopic)
        };
    }

    /// <summary>Builds a ProcessTelemetryCommand from a telemetry payload.</summary>
    private static Result<IRequest<Result>> ParseTelemetry(string serial, string payload, DateTimeOffset receivedAt)
    {
        if (Deserialize<TelemetryPayload>(payload) is not { } message)
        {
            return Result.Failure<IRequest<Result>>(IngestionErrors.InvalidJson);
        }

        var missing = new List<string>();
        var heartRate = Require(message.HeartRate, "heartRate", missing);
        var spo2 = Require(message.Spo2, "spo2", missing);
        var prIntervalMs = Require(message.PrIntervalMs, "prIntervalMs", missing);
        var qrsDurationMs = Require(message.QrsDurationMs, "qrsDurationMs", missing);
        var qtMs = Require(message.QtMs, "qtMs", missing);
        var qtcMs = Require(message.QtcMs, "qtcMs", missing);

        if (missing.Count > 0)
        {
            return Result.Failure<IRequest<Result>>(IngestionErrors.MissingFields(missing));
        }

        var featureVector = message.Features is { ValueKind: JsonValueKind.Object } features
            ? features.GetRawText()
            : null;

        return Result.Success<IRequest<Result>>(new ProcessTelemetryCommand(
            serial, message.Timestamp ?? receivedAt, heartRate, spo2, prIntervalMs, qrsDurationMs, qtMs, qtcMs,
            featureVector, message.EcgSamples));
    }

    /// <summary>Builds a ProcessDeviceStatusCommand from a status payload.</summary>
    private static Result<IRequest<Result>> ParseStatus(string serial, string payload)
    {
        if (Deserialize<StatusPayload>(payload) is not { } message)
        {
            return Result.Failure<IRequest<Result>>(IngestionErrors.InvalidJson);
        }

        var missing = new List<string>();
        var batteryLevel = Require(message.BatteryLevel, "batteryLevel", missing);

        if (!Enum.TryParse<ConnectionMode>(message.ConnectionMode, ignoreCase: true, out var connectionMode)
            || !Enum.IsDefined(connectionMode))
        {
            missing.Add("connectionMode");
        }

        if (missing.Count > 0)
        {
            return Result.Failure<IRequest<Result>>(IngestionErrors.MissingFields(missing));
        }

        return Result.Success<IRequest<Result>>(new ProcessDeviceStatusCommand(
            serial, batteryLevel, connectionMode, message.FirmwareVersion, message.SignalStrengthDbm));
    }

    /// <summary>Returns the value, or records the field as missing and returns 0 (caller checks the list).</summary>
    private static int Require(int? value, string fieldName, List<string> missing)
    {
        if (value is { } present)
        {
            return present;
        }

        missing.Add(fieldName);
        return 0;
    }

    /// <summary>Reads a JSON object; null when the text is not valid JSON or is the JSON literal null.</summary>
    private static T? Deserialize<T>(string payload)
        where T : class
    {
        try
        {
            return JsonSerializer.Deserialize<T>(payload, JsonOptions);
        }
        catch (JsonException)
        {
            return null;
        }
    }

    /// <summary>Wire shape of a telemetry message (everything optional so missing fields can be reported).</summary>
    private sealed record TelemetryPayload(
        DateTimeOffset? Timestamp,
        int? HeartRate,
        int? Spo2,
        int? PrIntervalMs,
        int? QrsDurationMs,
        int? QtMs,
        int? QtcMs,
        float[]? EcgSamples,
        JsonElement? Features);

    /// <summary>Wire shape of a status message.</summary>
    private sealed record StatusPayload(
        int? BatteryLevel, string? ConnectionMode, string? FirmwareVersion, int? SignalStrengthDbm);
}
