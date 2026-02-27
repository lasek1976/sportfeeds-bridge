using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;
using MongoDB.Driver.GridFS;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MongoDB.Bson.IO;
using SportFeedsBridge.Configuration;
using SportFeedsBridge.Phoenix.Models.Feeds;
using SportFeedsBridge.Phoenix.Serializers;
using SportFeedsBridge.Phoenix.Domain.Enums;
using FeedsType = SportFeedsBridge.Phoenix.Domain.Enums.FeedsType;
using System.Diagnostics;

namespace SportFeedsBridge.Services;

public class MongoDbReaderService
{
    private readonly ILogger<MongoDbReaderService> _logger;
    private readonly MongoDbSettings _settings;
    private readonly IMongoClient _client;
    private readonly IMongoDatabase _database;
    private readonly GridFSBucket _gridFsBucket;

    // Track last processed message IDs to avoid skipping messages
    private long _lastProcessedFixedMessageId = 0;
    private long _lastProcessedLiveMessageId = 0;

    public MongoDbReaderService(
        ILogger<MongoDbReaderService> logger,
        IOptions<MongoDbSettings> settings)
    {
        _logger = logger;
        _settings = settings.Value;

        // Initialize MongoDB client
        _client = new MongoClient(_settings.ConnectionString);

        // Get database name from connection string
        var mongoUrl = MongoUrl.Create(_settings.ConnectionString);
        var databaseName = mongoUrl.DatabaseName
            ?? throw new InvalidOperationException("Database name must be specified in ConnectionString");

        _database = _client.GetDatabase(databaseName);
        
        Console.WriteLine($"{DateTime.Now:HH:mm:ss.fff} [INF] {typeof(MongoDbReaderService).FullName}: Connected to MongoDB database: {databaseName}");
        _logger.LogInformation("Connected to MongoDB database: {DatabaseName}", databaseName);
        _gridFsBucket = new GridFSBucket(_database, new GridFSBucketOptions
        {
            BucketName = _settings.GridFSBucket
        });

        // Register MongoDB class maps
        RegisterClassMaps();
    }

    private void RegisterClassMaps()
    {
        try
        {
            if (!BsonClassMap.IsClassMapRegistered(typeof(FeedsMessage)))
            {
                var classMap = new MessageClassMap();
                classMap.Map();
                _logger.LogInformation("MongoDB class maps registered successfully");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to register MongoDB class maps");
            throw;
        }
    }

    /// <summary>
    /// Get latest Fixed Full message from GridFS (21_complete)
    /// </summary>
    public async Task<(FeedsMessage? Message, FeedsType FeedsType)> GetLatestFixedFullMessageAsync()
    {
        return await GetLatestFullMessageByTypeAsync(FeedsType.Fixed, "21_complete");
    }

    /// <summary>
    /// Get latest Live Full message from GridFS (22_complete)
    /// </summary>
    public async Task<(FeedsMessage? Message, FeedsType FeedsType)> GetLatestLiveFullMessageAsync()
    {
        return await GetLatestFullMessageByTypeAsync(FeedsType.Live, "22_complete");
    }

    /// <summary>
    /// Get latest Full message from GridFS by alias pattern
    /// </summary>
    private async Task<(FeedsMessage? Message, FeedsType FeedsType)> GetLatestFullMessageByTypeAsync(
        FeedsType feedsType,
        string aliasPattern)
    {
        try
        {
            var filesCollection = _database.GetCollection<GridFSFileInfo>($"{_settings.GridFSBucket}.files");

            var filter = Builders<GridFSFileInfo>.Filter.Regex(
                "metadata.Aliases",
                new BsonRegularExpression(aliasPattern));

            var file = await filesCollection
                .Find(filter)
                .SortByDescending(f => f.UploadDateTime)
                .Limit(1)
                .FirstOrDefaultAsync();

            if (file == null)
            {
                _logger.LogWarning("No {FeedsType} Full message found (alias: {Alias})",
                    feedsType, aliasPattern);
                return (null, feedsType);
            }

            _logger.LogInformation("Found {FeedsType} Full message: {FileName} ({Length} bytes)",
                feedsType, file.Filename, file.Length);

            // Log file metadata
            _logger.LogInformation("File metadata: {Metadata}", file.Metadata?.ToJson() ?? "null");

            // Download binary data
            var binaryData = await _gridFsBucket.DownloadAsBytesAsync(file.Id);

            _logger.LogInformation("Downloaded {Length} bytes from GridFS", binaryData.Length);

            // GridFS file contains raw protobuf-net serialized DataFeedsDiff (no compression, no wrapper)
            // Hex starting with C2 1F confirms this is protobuf wire format
            object body;
            try
            {
                _logger.LogInformation("Deserializing as raw protobuf-net DataFeedsDiff...");

                using (var stream = new MemoryStream(binaryData))
                {
                    // Deserialize directly as DataFeedsDiff using protobuf-net
                    body = ProtoBuf.Serializer.Deserialize<SportFeedsBridge.Phoenix.Models.Feeds.Diff.DataFeedsDiff>(stream);
                }

                _logger.LogInformation("Successfully deserialized DataFeedsDiff from GridFS");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to deserialize GridFS file data. Exception: {Message}", ex.Message);
                throw;
            }

            var message = new FeedsMessage
            {
                MessageId = ((ObjectId)file.Id).Timestamp,
                Body = body,
                CreatedTime = file.UploadDateTime,
                DiffType = file.Metadata?.GetValue("Aliases", "")?.AsString ?? "",
                Format = MessageFormat.Full
            };

            return (message, feedsType);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get latest {FeedsType} Full message", feedsType);
            throw;
        }
    }

    /// <summary>
    /// Get next unprocessed Fixed snapshot (processes messages sequentially from latest)
    /// On first run: gets latest message to catch up to current state (descending)
    /// After that: processes each new message sequentially in order (ascending)
    /// </summary>
    public async Task<(FeedsMessage? Message, FeedsType FeedsType)> GetLatestFixedSnapshotAsync()
    {
        try
        {
            var collection = _database.GetCollection<SnapshotMessage>(_settings.FixedSnapshotMessagesCollection);

            // On first run (lastProcessed == 0): use descending to get latest message
            // On subsequent runs: use ascending to process messages in order
            bool isFirstRun = _lastProcessedFixedMessageId == 0;

            var query = collection.Find(m =>
                m.FeedsType == SportFeedsBridge.Phoenix.Domain.Enums.FeedsType.Fixed &&
                m.MessageId > _lastProcessedFixedMessageId);

            var swPointer = Stopwatch.StartNew();
            var pointer = isFirstRun
                ? await query.SortByDescending(m => m.MessageId).Limit(1).FirstOrDefaultAsync()  // First run: get latest
                : await query.SortBy(m => m.MessageId).Limit(1).FirstOrDefaultAsync();            // Normal: get next in sequence
            swPointer.Stop();

            if (pointer == null)
            {
                // No new messages (all caught up)
                return (null, FeedsType.Fixed);
            }

            _logger.LogInformation("Found Fixed snapshot pointer: MessageId={MessageId} (last processed: {LastProcessed}, first run: {IsFirstRun}) [pointer: {PointerMs}ms]",
                pointer.MessageId, _lastProcessedFixedMessageId, isFirstRun, swPointer.ElapsedMilliseconds);

            // Get the actual message from FeedsMessages collection
            var message = await GetSnapshotByIdAsync(pointer.MessageId);

            // Update last processed ID only if message was retrieved successfully
            if (message != null)
            {
                _lastProcessedFixedMessageId = pointer.MessageId;
            }

            return (message, FeedsType.Fixed);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get next Fixed snapshot");
            throw;
        }
    }

    /// <summary>
    /// Get next unprocessed Live snapshot (processes messages sequentially from latest)
    /// On first run: gets latest message to catch up to current state (descending)
    /// After that: processes each new message sequentially in order (ascending)
    /// </summary>
    public async Task<(FeedsMessage? Message, FeedsType FeedsType)> GetLatestLiveSnapshotAsync()
    {
        try
        {
            var collection = _database.GetCollection<SnapshotMessage>(_settings.LiveSnapshotMessagesCollection);

            // On first run (lastProcessed == 0): use descending to get latest message
            // On subsequent runs: use ascending to process messages in order
            bool isFirstRun = _lastProcessedLiveMessageId == 0;

            var query = collection.Find(m =>
                m.FeedsType == FeedsType.Live &&
                m.MessageId > _lastProcessedLiveMessageId);

            var swPointer = Stopwatch.StartNew();
            var pointer = isFirstRun
                ? await query.SortByDescending(m => m.MessageId).Limit(1).FirstOrDefaultAsync()  // First run: get latest
                : await query.SortBy(m => m.MessageId).Limit(1).FirstOrDefaultAsync();            // Normal: get next in sequence
            swPointer.Stop();

            if (pointer == null)
            {
                // No new messages (all caught up)
                return (null, FeedsType.Live);
            }

            _logger.LogInformation("Found Live snapshot pointer: MessageId={MessageId} (last processed: {LastProcessed}, first run: {IsFirstRun}) [pointer: {PointerMs}ms]",
                pointer.MessageId, _lastProcessedLiveMessageId, isFirstRun, swPointer.ElapsedMilliseconds);

            // Get the actual message from FeedsMessages collection
            var message = await GetSnapshotByIdAsync(pointer.MessageId);

            // Update last processed ID only if message was retrieved successfully
            if (message != null)
            {
                _lastProcessedLiveMessageId = pointer.MessageId;
            }

            return (message, FeedsType.Live);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get next Live snapshot");
            throw;
        }
    }

    /// <summary>
    /// Get Snapshot message by ID
    /// </summary>
    public async Task<FeedsMessage?> GetSnapshotByIdAsync(long messageId)
    {
        try
        {
            var collection = _database.GetCollection<FeedsMessage>(_settings.FeedsMessagesCollection);

            var swFetch = Stopwatch.StartNew();
            var message = await collection
                .Find(m => m.MessageId == messageId)
                .FirstOrDefaultAsync();
            swFetch.Stop();

            if (message == null)
            {
                _logger.LogWarning("Snapshot message {MessageId} not found", messageId);
                return null;
            }

            _logger.LogInformation("Found Snapshot message: {MessageId} (Format: {Format}) [fetch: {FetchMs}ms]",
                message.MessageId, message.Format, swFetch.ElapsedMilliseconds);

            return message;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get Snapshot {MessageId}", messageId);
            throw;
        }
    }

    /// <summary>
    /// Get a GridFS Full message by its file ObjectId hex string.
    /// Uses the same raw protobuf-net deserialization as the regular Full message pipeline.
    /// </summary>
    public async Task<(FeedsMessage? Message, FeedsType FeedsType)> GetFullMessageByFileIdAsync(string fileId)
    {
        try
        {
            var objId = new MongoDB.Bson.ObjectId(fileId);

            var filesCollection = _database.GetCollection<GridFSFileInfo>($"{_settings.GridFSBucket}.files");
            var file = await filesCollection
                .Find(Builders<GridFSFileInfo>.Filter.Eq("_id", objId))
                .FirstOrDefaultAsync();

            if (file == null)
            {
                _logger.LogWarning("GridFS file not found: {FileId}", fileId);
                return (null, FeedsType.Fixed);
            }

            _logger.LogInformation("Downloading GridFS file {FileName} ({Length} bytes)", file.Filename, file.Length);

            var binaryData = await _gridFsBucket.DownloadAsBytesAsync(objId);

            _logger.LogInformation("Downloaded {Length} bytes, deserializing as protobuf-net DataFeedsDiff", binaryData.Length);

            object body;
            using (var stream = new MemoryStream(binaryData))
            {
                body = ProtoBuf.Serializer.Deserialize<SportFeedsBridge.Phoenix.Models.Feeds.Diff.DataFeedsDiff>(stream);
            }

            var feedsType = file.Metadata?.GetValue("Aliases", "")?.AsString?.Contains("21_") == true
                ? FeedsType.Fixed
                : FeedsType.Live;

            var message = new FeedsMessage
            {
                MessageId = objId.Timestamp,
                Body = body,
                CreatedTime = file.UploadDateTime,
                DiffType = file.Metadata?.GetValue("Aliases", "")?.AsString ?? "",
                Format = MessageFormat.Full
            };

            _logger.LogInformation("Successfully deserialized GridFS file {FileId}", fileId);
            return (message, feedsType);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get GridFS file {FileId}", fileId);
            throw;
        }
    }

    /// <summary>
    /// Reset message tracking (useful when sending Full messages or restarting)
    /// </summary>
    public void ResetMessageTracking()
    {
        _logger.LogInformation("Resetting message tracking (Fixed: {Fixed} → 0, Live: {Live} → 0)",
            _lastProcessedFixedMessageId, _lastProcessedLiveMessageId);
        _lastProcessedFixedMessageId = 0;
        _lastProcessedLiveMessageId = 0;
    }

    /// <summary>
    /// Get current tracking state for diagnostics
    /// </summary>
    public (long LastProcessedFixed, long LastProcessedLive) GetMessageTrackingState()
    {
        return (_lastProcessedFixedMessageId, _lastProcessedLiveMessageId);
    }

}
