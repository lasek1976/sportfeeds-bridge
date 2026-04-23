using RabbitMQ.Client;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SportFeedsBridge.Configuration;
using SportFeedsBridge.Phoenix.Models.Feeds;
using SportFeedsBridge.Phoenix.Models.Feeds.Diff;
using SportFeedsBridge.Phoenix.Domain.Enums;
using Google.Protobuf;
using System.Diagnostics;

namespace SportFeedsBridge.Services;

public class RabbitMQPublisherService : IDisposable
{
    private readonly ILogger<RabbitMQPublisherService> _logger;
    private readonly RabbitMQSettings _settings;
    private readonly ProcessingSettings _processingSettings;
    private readonly IConnection _connection;
    private readonly IChannel _channel;
    private readonly DebugProtoBufService _debugService;
    private static bool _testMessageSaved = false;

    public RabbitMQPublisherService(
        ILogger<RabbitMQPublisherService> logger,
        IOptions<RabbitMQSettings> settings,
        IOptions<ProcessingSettings> processingSettings,
        DebugProtoBufService debugService)
    {
        _logger = logger;
        _settings = settings.Value;
        _processingSettings = processingSettings.Value;
        _debugService = debugService;

        // Create RabbitMQ connection
        var factory = new ConnectionFactory
        {
            HostName = _settings.HostName,
            Port = _settings.Port,
            UserName = _settings.UserName,
            Password = _settings.Password,
            VirtualHost = _settings.VirtualHost
        };

        _connection = factory.CreateConnectionAsync().GetAwaiter().GetResult();
        _channel = _connection.CreateChannelAsync().GetAwaiter().GetResult();

        InitializeRabbitMQAsync().GetAwaiter().GetResult();
    }

    private async Task InitializeRabbitMQAsync()
    {
        try
        {
            // Declare exchange (topic type for routing flexibility)
            await _channel.ExchangeDeclareAsync(
                exchange: _settings.ExchangeName,
                type: ExchangeType.Topic,
                durable: true,
                autoDelete: false);

            // Declare Fixed queue
            await _channel.QueueDeclareAsync(
                queue: _settings.FixedQueueName,
                durable: true,
                exclusive: false,
                autoDelete: false);

            // Bind Fixed queue to exchange
            await _channel.QueueBindAsync(
                queue: _settings.FixedQueueName,
                exchange: _settings.ExchangeName,
                routingKey: _settings.FixedRoutingKey);

            // Declare Live queue
            await _channel.QueueDeclareAsync(
                queue: _settings.LiveQueueName,
                durable: true,
                exclusive: false,
                autoDelete: false);

            // Bind Live queue to exchange
            await _channel.QueueBindAsync(
                queue: _settings.LiveQueueName,
                exchange: _settings.ExchangeName,
                routingKey: _settings.LiveRoutingKey);

            _logger.LogInformation(
                "RabbitMQ initialized - Exchange: {Exchange}, Fixed Queue: {FixedQueue} ({FixedKey}), Live Queue: {LiveQueue} ({LiveKey})",
                _settings.ExchangeName, _settings.FixedQueueName, _settings.FixedRoutingKey,
                _settings.LiveQueueName, _settings.LiveRoutingKey);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to initialize RabbitMQ");
            throw;
        }
    }

    /// <summary>
    /// Publish FeedsMessage to RabbitMQ using ProtoBuf serialization
    /// </summary>
    public async Task PublishMessageAsync(FeedsMessage message, FeedsType feedsType)
    {
        try
        {
            byte[] body;
            string contentType;
            string routingKey;

            // Determine routing key based on FeedsType
            routingKey = feedsType == FeedsType.Fixed
                ? _settings.FixedRoutingKey
                : _settings.LiveRoutingKey;

            /* TEST: Verify serialization/deserialization works (only log on first message)
            if (message.Body != null)
            {
                // Save test message once for JavaScript debugging
                if (!_testMessageSaved && message.Body is DataFeedsDiff feedsDiff)
                {
                    _debugService.SaveTestMessage(feedsDiff);
                    _testMessageSaved = true;
                }
                
                var testResult = TestProtoBufRoundtrip(message.Body);
                if (!testResult)
                {
                    _logger.LogWarning("ProtoBuf roundtrip test failed! Publishing anyway (Events conversion not yet implemented).");
                }
            }
            */

            // Serialize using ProtoBuf (standard proto3, not protobuf-net BCL)
            var swSerialize = Stopwatch.StartNew();
            body = SerializeToProtoBuf(message.Body);
            swSerialize.Stop();
            contentType = "application/protobuf";

            var properties = new BasicProperties
            {
                ContentType = contentType,
                DeliveryMode = DeliveryModes.Persistent,
                Timestamp = new AmqpTimestamp(DateTimeOffset.UtcNow.ToUnixTimeSeconds()),
                MessageId = message.MessageId.ToString(),
                Headers = new Dictionary<string, object?>
                {
                    ["MessageId"] = message.MessageId,
                    ["DiffType"] = message.DiffType ?? "",
                    ["Format"] = message.Format.ToString(),
                    ["CreatedTime"] = message.CreatedTime.ToString("O"),
                    ["MessageType"] = message.Body?.GetType().Name ?? "Unknown",
                    ["FeedsType"] = feedsType.ToString()
                }
            };

            var swPublish = Stopwatch.StartNew();
            await _channel.BasicPublishAsync(
                exchange: _settings.ExchangeName,
                routingKey: routingKey,
                mandatory: true,
                basicProperties: properties,
                body: body);
            swPublish.Stop();

            _logger.LogInformation(
                "Published {FeedsType} message {MessageId} to RabbitMQ ({Size} bytes, routing: {RoutingKey}) [serialize: {SerializeMs}ms, publish: {PublishMs}ms]",
                feedsType, message.MessageId, body.Length, routingKey,
                swSerialize.ElapsedMilliseconds, swPublish.ElapsedMilliseconds);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to publish {FeedsType} message {MessageId}", feedsType, message.MessageId);
            throw;
        }
    }

    /// <summary>
    /// Test serialization/deserialization roundtrip
    /// </summary>
    public bool TestProtoBufRoundtrip(object obj)
    {
        try
        {
            _logger.LogInformation("=== Testing ProtoBuf Roundtrip (Google.Protobuf) ===");

            // Serialize
            var serialized = SerializeToProtoBuf(obj);
            _logger.LogInformation("Serialized {Size} bytes", serialized.Length);

            // Hex dump removed for cleaner console output

            // Deserialize back using Google.Protobuf
            var deserialized = Sportfeeds.DataFeedsDiff.Parser.ParseFrom(serialized);

            _logger.LogInformation("✓ Deserialized successfully!");
            _logger.LogInformation("  - Events: {EventCount}", deserialized.Events?.Count ?? 0);
            _logger.LogInformation("  - DiffType: {DiffType}", deserialized.DiffType);
            _logger.LogInformation("  - CreatedUTCTime: {Time}", deserialized.CreatedUTCTime?.ToDateTime());

            // Check if event properties are populated
            if (deserialized.Events?.Count > 0)
            {
                var firstEvent = deserialized.Events[0];
                _logger.LogInformation("  - First Event Properties (DataEventDiff):");
                _logger.LogInformation("    - IDEvent: {IDEvent}", firstEvent.IDEvent);
                _logger.LogInformation("    - EventName: '{EventName}'", firstEvent.EventName ?? "(null)");
                _logger.LogInformation("    - SportName: '{SportName}'", firstEvent.SportName ?? "(null)");
                _logger.LogInformation("    - TournamentName: '{TournamentName}'", firstEvent.TournamentName ?? "(null)");
                _logger.LogInformation("    - IDSport: {IDSport}", firstEvent.IDSport);
                _logger.LogInformation("    - EventDate: {EventDate}", firstEvent.EventDate?.ToDateTime());
                _logger.LogInformation("    - DiffType: {DiffType}", firstEvent.DiffType);
                _logger.LogInformation("    - Markets Count: {MarketsCount}", firstEvent.Markets?.Count ?? 0);
                _logger.LogInformation("    - Teams Count: {TeamsCount}", firstEvent.Teams?.Count ?? 0);
                _logger.LogInformation("    - ScoreBoards Count: {ScoreBoardsCount}", firstEvent.ScoreBoards?.Count ?? 0);

                if (firstEvent.Markets?.Count > 0)
                {
                    var firstMarket = firstEvent.Markets[0];
                    _logger.LogInformation("    - First Market: IDMarket={IDMarket}, Selections={SelectionsCount}",
                        firstMarket.IDMarket, firstMarket.Selections?.Count ?? 0);
                }

                if (firstEvent.Teams?.Count > 0)
                {
                    var firstTeam = firstEvent.Teams[0];
                    _logger.LogInformation("    - First Team: TeamName='{TeamName}', TeamId={TeamId}",
                        firstTeam.TeamName, firstTeam.TeamId);
                }

                _logger.LogInformation("    ✓ Event properties are POPULATED!");
            }

            // Verify event count matches
            if (obj is DataFeedsDiff original)
            {
                var originalCount = original.Events?.Count ?? 0;
                var deserializedCount = deserialized.Events?.Count ?? 0;

                if (originalCount == deserializedCount)
                {
                    _logger.LogInformation("✓ Event count matches: {Count}", originalCount);
                    return true;
                }
                else
                {
                    _logger.LogError("✗ Event count mismatch! Original: {Original}, Deserialized: {Deserialized}",
                        originalCount, deserializedCount);
                    return false;
                }
            }

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "✗ ProtoBuf roundtrip test FAILED");
            return false;
        }
    }

    /// <summary>
    /// Serialize object to ProtoBuf using Google.Protobuf (standard proto3 format)
    /// This produces clean ProtoBuf compatible with any ProtoBuf library (including JavaScript)
    /// </summary>
    private byte[] SerializeToProtoBuf(object obj)
    {
        using var ms = new MemoryStream();

        // Convert Phoenix models to Google.Protobuf generated models
        if (obj is DataFeedsDiff feedsDiff)
        {
            // Convert to protobuf-generated model (no inheritance, standard proto3)
            Sportfeeds.DataFeedsDiff protobufModel = ProtobufConverter.ToProtobuf(feedsDiff);
            
            _logger.LogDebug("Serializing DataFeedsDiff (Google.Protobuf) with {EventCount} events",
                protobufModel.Events?.Count ?? 0);

            // Serialize using Google.Protobuf
            protobufModel.WriteTo(ms);
        }
        else
        {
            _logger.LogWarning("Unknown type: {TypeName}, cannot serialize",
                obj.GetType().FullName);
            throw new InvalidOperationException($"Cannot serialize type {obj.GetType().FullName}");
        }

        return ms.ToArray();
    }

    public void Dispose()
    {
        try
        {
            _channel?.Dispose();
            _connection?.Dispose();
            _logger.LogInformation("RabbitMQ connection closed");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error disposing RabbitMQ connection");
        }
    }
}
