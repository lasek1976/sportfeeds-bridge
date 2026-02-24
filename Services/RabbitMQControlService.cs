using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Text;
using SportFeedsBridge.Configuration;

namespace SportFeedsBridge.Services;

/// <summary>
/// Listens to RabbitMQ control queue for commands (e.g., "send-full")
/// </summary>
public class RabbitMQControlService : IDisposable
{
    private readonly ILogger<RabbitMQControlService> _logger;
    private readonly RabbitMQSettings _settings;
    private readonly IConnection _connection;
    private readonly IChannel _channel;
    private const string ControlQueueName = "sportfeeds.control";

    public event Func<string, Task>? OnControlMessage;

    public RabbitMQControlService(
        ILogger<RabbitMQControlService> logger,
        IOptions<RabbitMQSettings> settings)
    {
        _logger = logger;
        _settings = settings.Value;

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

        InitializeAsync().GetAwaiter().GetResult();
    }

    private async Task InitializeAsync()
    {
        // Declare control queue
        await _channel.QueueDeclareAsync(
            queue: ControlQueueName,
            durable: false,
            exclusive: false,
            autoDelete: false,
            arguments: null);

        _logger.LogInformation("Control queue declared: {QueueName}", ControlQueueName);

        // Start consuming
        var consumer = new AsyncEventingBasicConsumer(_channel);
        consumer.ReceivedAsync += async (model, ea) =>
        {
            try
            {
                var body = ea.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);

                _logger.LogInformation("Received control message: {Message}", message);

                if (OnControlMessage != null)
                {
                    await OnControlMessage.Invoke(message);
                }

                await _channel.BasicAckAsync(deliveryTag: ea.DeliveryTag, multiple: false);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing control message");
            }
        };

        await _channel.BasicConsumeAsync(
            queue: ControlQueueName,
            autoAck: false,
            consumer: consumer);

        _logger.LogInformation("Started listening to control queue");
    }

    public void Dispose()
    {
        _channel?.Dispose();
        _connection?.Dispose();
    }
}
