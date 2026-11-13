// =============================================================================
// File           : MqttIngestionService.cs
// Layer          : Infrastructure / Ingestion
// Purpose        : Background worker that receives device messages from the MQTT broker and feeds the application.
// Logic          : Connects (TLS by default), subscribes to devices/+/telemetry and devices/+/status, and keeps
//                  reconnecting for as long as the host runs. Each message is parsed, then sent to the mediator in its
//                  own DI scope, so validation, the device check and the whole pipeline of ProcessTelemetryHandler
//                  apply exactly as for any other request. A malformed or rejected message is logged and dropped
//                  (never retried); one bad message cannot stop the worker.
// Implementation : BackgroundService hosted inside the Api process in Phase 1 (it can move to its own worker host
//                  later without code changes). Subscriptions use QoS 1 with a clean session: while the worker is
//                  down nothing is queued, because stale vitals are of no use. MQTTnet is used only in this file.
// =============================================================================
using MQTTnet;
using MQTTnet.Formatter;
using MQTTnet.Protocol;
using Mediator;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace CardioSense.Infrastructure.Ingestion;

/// <summary>Receives MQTT messages from devices.</summary>
internal sealed class MqttIngestionService(
    IOptions<MqttOptions> options,
    IServiceScopeFactory scopes,
    TimeProvider clock,
    ILogger<MqttIngestionService> logger)
    : BackgroundService
{
    /// <summary>Largest accepted message; anything bigger is dropped without being read.</summary>
    private const int MaxPayloadBytes = 1_048_576;

    /// <summary>Keeps the connection alive until the host stops.</summary>
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var settings = options.Value;
        if (!settings.Enabled)
        {
            logger.LogInformation("MQTT ingestion is disabled (Mqtt:Enabled is false).");
            return;
        }

        using var client = new MqttClientFactory().CreateMqttClient();
        client.ApplicationMessageReceivedAsync += eventArgs => HandleMessageAsync(eventArgs, stoppingToken);

        var connectOptions = BuildConnectOptions(settings);
        var subscribeOptions = BuildSubscribeOptions();
        var retryDelay = TimeSpan.FromSeconds(settings.ReconnectDelaySeconds);

        try
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                if (!client.IsConnected)
                {
                    await TryConnectAsync(client, connectOptions, subscribeOptions, stoppingToken);
                }

                await Task.Delay(retryDelay, stoppingToken);
            }
        }
        catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
        {
            logger.LogInformation("MQTT ingestion is stopping.");
        }

        await DisconnectQuietlyAsync(client);
    }

    /// <summary>Connects and subscribes; on failure the client is left disconnected so the loop retries.</summary>
    private async Task TryConnectAsync(
        IMqttClient client, MqttClientOptions connectOptions, MqttClientSubscribeOptions subscribeOptions,
        CancellationToken ct)
    {
        try
        {
            await client.ConnectAsync(connectOptions, ct);
            await client.SubscribeAsync(subscribeOptions, ct);
            logger.LogInformation("MQTT ingestion connected and subscribed.");
        }
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
            logger.LogWarning(exception, "MQTT connection attempt failed; it will be retried.");
            await DisconnectQuietlyAsync(client);
        }
    }

    /// <summary>Parses one message and runs it through the application pipeline; never throws.</summary>
    private async Task HandleMessageAsync(MqttApplicationMessageReceivedEventArgs eventArgs, CancellationToken ct)
    {
        var message = eventArgs.ApplicationMessage;

        if (message.Payload.Length > MaxPayloadBytes)
        {
            logger.LogWarning("Dropped an oversized MQTT message on {Topic}.", message.Topic);
            return;
        }

        var parsed = TelemetryMessageParser.Parse(message.Topic, message.ConvertPayloadToString(), clock.GetUtcNow());
        if (parsed.IsFailure)
        {
            logger.LogWarning("Dropped a malformed MQTT message on {Topic}: {ErrorCode}.", message.Topic,
                parsed.Error.Code);
            return;
        }

        try
        {
            await using var scope = scopes.CreateAsyncScope();
            var sender = scope.ServiceProvider.GetRequiredService<ISender>();
            var outcome = await sender.Send(parsed.Value, ct);

            if (outcome.IsFailure)
            {
                logger.LogWarning("MQTT message on {Topic} was rejected: {ErrorCode}.", message.Topic,
                    outcome.Error.Code);
            }
        }
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
            logger.LogError(exception, "Processing the MQTT message on {Topic} failed.", message.Topic);
        }
    }

    /// <summary>Builds the connection options (host, TLS, credentials, client id).</summary>
    private static MqttClientOptions BuildConnectOptions(MqttOptions settings)
    {
        var builder = new MqttClientOptionsBuilder()
            .WithTcpServer(settings.BrokerHost, settings.BrokerPort)
            .WithProtocolVersion(MqttProtocolVersion.V311)
            .WithClientId(settings.ClientId);

        if (settings.UseTls)
        {
            builder.WithTlsOptions(tls => tls.UseTls());
        }

        if (!string.IsNullOrEmpty(settings.Username))
        {
            builder.WithCredentials(settings.Username, settings.Password);
        }

        return builder.Build();
    }

    /// <summary>Builds the subscription to the telemetry and status topics of every device.</summary>
    private static MqttClientSubscribeOptions BuildSubscribeOptions() =>
        new MqttClientSubscribeOptionsBuilder()
            .WithTopicFilter(TelemetryMessageParser.TelemetryTopicFilter, MqttQualityOfServiceLevel.AtLeastOnce)
            .WithTopicFilter(TelemetryMessageParser.StatusTopicFilter, MqttQualityOfServiceLevel.AtLeastOnce)
            .Build();

    /// <summary>Disconnects if connected and swallows (but logs) any error, so cleanup never throws.</summary>
    private async Task DisconnectQuietlyAsync(IMqttClient client)
    {
        if (!client.IsConnected)
        {
            return;
        }

        try
        {
            await client.DisconnectAsync();
        }
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
            logger.LogDebug(exception, "MQTT disconnect reported an error.");
        }
    }
}
