using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SportFeedsBridge.Configuration;
using System.Diagnostics;

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
            await SendFixedFullAsync();

        if (_settings.ProcessLive)
            await SendLiveFullAsync();
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Bridge Worker Service started (Fixed: {ProcessFixed}, Live: {ProcessLive})",
            _settings.ProcessFixed, _settings.ProcessLive);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                // On first run, send Full messages — retry each poll cycle until both are available
                if (_isFirstRun)
                {
                    _logger.LogInformation("First run - sending Full messages for bootstrap");

                    bool fixedOk = !_settings.ProcessFixed || await SendFixedFullAsync();
                    bool liveOk  = !_settings.ProcessLive  || await SendLiveFullAsync();

                    if (fixedOk && liveOk)
                    {
                        _isFirstRun = false;
                    }
                    else
                    {
                        _logger.LogWarning("Full message(s) not yet available — will retry on next poll cycle");
                        await Task.Delay(TimeSpan.FromSeconds(_settings.PollingIntervalSeconds), stoppingToken);
                        continue;
                    }
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
    /// Returns true if message was found and sent, false if not available yet.
    /// </summary>
    private async Task<bool> SendFixedFullAsync()
    {
        try
        {
            var (fullMessage, fullFeedsType) = await _mongoReader.GetLatestFixedFullMessageAsync();
            if (fullMessage == null)
                return false;

            var feedsDiff = fullMessage.Body as SportFeedsBridge.Phoenix.Models.Feeds.Diff.DataFeedsDiff;
            var eventCount = feedsDiff?.Events?.Count ?? 0;
            _logger.LogInformation("Sending Fixed Full message: {MessageId} | Events: {EventCount}", fullMessage.MessageId, eventCount);

            if (feedsDiff != null)
            {
                _debugService.SaveEventsBySport(feedsDiff);
            }

            await _rabbitPublisher.PublishMessageAsync(fullMessage, fullFeedsType);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send Fixed Full message");
            return false;
        }
    }

    /// <summary>
    /// Send Live Full message (called at startup or on manual request)
    /// Returns true if message was found and sent, false if not available yet.
    /// </summary>
    private async Task<bool> SendLiveFullAsync()
    {
        try
        {
            var (fullMessage, fullFeedsType) = await _mongoReader.GetLatestLiveFullMessageAsync();
            if (fullMessage == null)
                return false;

            var eventCount = (fullMessage.Body as SportFeedsBridge.Phoenix.Models.Feeds.Diff.DataFeedsDiff)?.Events?.Count ?? 0;
            _logger.LogInformation("Sending Live Full message: {MessageId} | Events: {EventCount}", fullMessage.MessageId, eventCount);
            await _rabbitPublisher.PublishMessageAsync(fullMessage, fullFeedsType);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send Live Full message");
            return false;
        }
    }

    /// <summary>
    /// Process Fixed Snapshot messages (continuous polling)
    /// </summary>
    private async Task ProcessFixedSnapshotsAsync()
    {
        try
        {
            var totalSw = Stopwatch.StartNew();
            var (snapshotMessage, snapshotFeedsType) = await _mongoReader.GetLatestFixedSnapshotAsync();
            var mongoMs = totalSw.ElapsedMilliseconds;

            if (snapshotMessage != null)
            {
                var eventCount = (snapshotMessage.Body as SportFeedsBridge.Phoenix.Models.Feeds.Diff.DataFeedsDiff)?.Events?.Count ?? 0;
                _logger.LogInformation("Processing Fixed Snapshot message: {MessageId} | Events: {EventCount} [mongo: {MongoMs}ms]",
                    snapshotMessage.MessageId, eventCount, mongoMs);
                await _rabbitPublisher.PublishMessageAsync(snapshotMessage, snapshotFeedsType);
                _logger.LogInformation("Fixed Snapshot {MessageId} end-to-end: {TotalMs}ms",
                    snapshotMessage.MessageId, totalSw.ElapsedMilliseconds);
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
            var totalSw = Stopwatch.StartNew();
            var (snapshotMessage, snapshotFeedsType) = await _mongoReader.GetLatestLiveSnapshotAsync();
            var mongoMs = totalSw.ElapsedMilliseconds;

            if (snapshotMessage != null)
            {
                var eventCount = (snapshotMessage.Body as SportFeedsBridge.Phoenix.Models.Feeds.Diff.DataFeedsDiff)?.Events?.Count ?? 0;
                _logger.LogInformation("Processing Live Snapshot message: {MessageId} | Events: {EventCount} [mongo: {MongoMs}ms]",
                    snapshotMessage.MessageId, eventCount, mongoMs);
                await _rabbitPublisher.PublishMessageAsync(snapshotMessage, snapshotFeedsType);
                _logger.LogInformation("Live Snapshot {MessageId} end-to-end: {TotalMs}ms",
                    snapshotMessage.MessageId, totalSw.ElapsedMilliseconds);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to process Live snapshots");
        }
    }
}
