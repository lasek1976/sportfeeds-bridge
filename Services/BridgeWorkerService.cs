using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SportFeedsBridge.Configuration;

namespace SportFeedsBridge.Services;

/// <summary>
/// Background service that polls MongoDB and publishes to RabbitMQ
/// Handles both Fixed and Live message types
/// </summary>
public class BridgeWorkerService : BackgroundService
{
    private readonly ILogger<BridgeWorkerService> _logger;
    private readonly MongoDbReaderService _mongoReader;
    private readonly RabbitMQPublisherService _rabbitPublisher;
    private readonly RabbitMQControlService _controlService;
    private readonly DebugProtoBufService _debugService;
    private readonly ProcessingSettings _settings;

    // Track if initial Full messages have been sent at startup
    private bool _isFirstRun = true;

    public BridgeWorkerService(
        ILogger<BridgeWorkerService> logger,
        MongoDbReaderService mongoReader,
        RabbitMQPublisherService rabbitPublisher,
        RabbitMQControlService controlService,
        DebugProtoBufService debugService,
        IOptions<ProcessingSettings> settings)
    {
        _logger = logger;
        _mongoReader = mongoReader;
        _rabbitPublisher = rabbitPublisher;
        _controlService = controlService;
        _debugService = debugService;
        _settings = settings.Value;

        // Listen for control messages
        _controlService.OnControlMessage += HandleControlMessageAsync;
    }

    /// <summary>
    /// Handle control messages from RabbitMQ control queue
    /// </summary>
    private async Task HandleControlMessageAsync(string message)
    {
        _logger.LogInformation("Processing control message: {Message}", message);

        if (message == "send-full")
        {
            await SendFullMessagesAsync();
        }
    }

    /// <summary>
    /// Manually trigger sending Full messages (called from control message)
    /// </summary>
    private async Task SendFullMessagesAsync()
    {
        _logger.LogInformation("Manually triggering Full messages send...");

        if (_settings.ProcessFixed)
        {
            await SendFixedFullAsync();
        }

        if (_settings.ProcessLive)
        {
            await SendLiveFullAsync();
        }
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Bridge Worker Service started (Fixed: {ProcessFixed}, Live: {ProcessLive})",
            _settings.ProcessFixed, _settings.ProcessLive);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                // On first run, send Full messages
                if (_isFirstRun)
                {
                    _logger.LogInformation("First run - sending Full messages for bootstrap");

                    if (_settings.ProcessFixed)
                    {
                        await SendFixedFullAsync();
                    }

                    if (_settings.ProcessLive)
                    {
                        await SendLiveFullAsync();
                    }

                    _isFirstRun = false;
                }

                // Process Fixed Snapshot messages (continuous)
                if (_settings.ProcessFixed)
                {
                    await ProcessFixedSnapshotsAsync();
                }

                // Process Live Snapshot messages (continuous)
                if (_settings.ProcessLive)
                {
                    await ProcessLiveSnapshotsAsync();
                }

                // Wait before next poll
                await Task.Delay(
                    TimeSpan.FromSeconds(_settings.PollingIntervalSeconds),
                    stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in Bridge Worker Service");
                await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken);
            }
        }

        _logger.LogInformation("Bridge Worker Service stopped");
    }

    /// <summary>
    /// Send Fixed Full message (called at startup or on manual request)
    /// </summary>
    private async Task SendFixedFullAsync()
    {
        try
        {
            var (fullMessage, fullFeedsType) = await _mongoReader.GetLatestFixedFullMessageAsync();
            if (fullMessage != null)
            {
                _logger.LogInformation("Sending Fixed Full message: {MessageId}", fullMessage.MessageId);

                // Debug: Save events for sports 28, 12, and 64 to JSON for inspection
                if (fullMessage.Body is SportFeedsBridge.Phoenix.Models.Feeds.Diff.DataFeedsDiff feedsDiff)
                {
                    _debugService.SaveEventsBySport(feedsDiff, 28, 12, 64);
                }

                await _rabbitPublisher.PublishMessageAsync(fullMessage, fullFeedsType);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send Fixed Full message");
        }
    }

    /// <summary>
    /// Send Live Full message (called at startup or on manual request)
    /// </summary>
    private async Task SendLiveFullAsync()
    {
        try
        {
            var (fullMessage, fullFeedsType) = await _mongoReader.GetLatestLiveFullMessageAsync();
            if (fullMessage != null)
            {
                _logger.LogInformation("Sending Live Full message: {MessageId}", fullMessage.MessageId);
                await _rabbitPublisher.PublishMessageAsync(fullMessage, fullFeedsType);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send Live Full message");
        }
    }

    /// <summary>
    /// Process Fixed Snapshot messages (continuous polling)
    /// </summary>
    private async Task ProcessFixedSnapshotsAsync()
    {
        try
        {
            var (snapshotMessage, snapshotFeedsType) = await _mongoReader.GetLatestFixedSnapshotAsync();
            if (snapshotMessage != null)
            {
                _logger.LogInformation("Processing Fixed Snapshot message: {MessageId}", snapshotMessage.MessageId);
                await _rabbitPublisher.PublishMessageAsync(snapshotMessage, snapshotFeedsType);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to process Fixed snapshots");
        }
    }

    /// <summary>
    /// Process Live Snapshot messages (continuous polling)
    /// </summary>
    private async Task ProcessLiveSnapshotsAsync()
    {
        try
        {
            var (snapshotMessage, snapshotFeedsType) = await _mongoReader.GetLatestLiveSnapshotAsync();
            if (snapshotMessage != null)
            {
                _logger.LogInformation("Processing Live Snapshot message: {MessageId}", snapshotMessage.MessageId);
                await _rabbitPublisher.PublishMessageAsync(snapshotMessage, snapshotFeedsType);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to process Live snapshots");
        }
    }
}
