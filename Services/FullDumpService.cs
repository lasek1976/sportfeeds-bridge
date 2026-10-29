using Google.Protobuf;
using Microsoft.Extensions.Logging;
using SportFeedsBridge.Phoenix.Domain.Enums;
using SportFeedsBridge.Phoenix.Models.Feeds.Diff;

namespace SportFeedsBridge.Services;

/// <summary>
/// Saves the Full messages read at startup as JSON under full/&lt;yyyy-MM-dd_HH-mm-ss&gt;,
/// so the bootstrap state stays inspectable after old Fulls are purged from MongoDB.
/// </summary>
public class FullDumpService
{
    private readonly ILogger<FullDumpService> _logger;
    private readonly string _dumpPath;

    // Same formatter as the /api/message endpoints: include zero/false/empty fields
    private static readonly JsonFormatter JsonFormatter =
        new(JsonFormatter.Settings.Default.WithFormatDefaultValues(true));

    public FullDumpService(ILogger<FullDumpService> logger)
    {
        _logger = logger;
        _dumpPath = Path.Combine(Directory.GetCurrentDirectory(), "full", DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss"));
    }

    /// <summary>
    /// Write the Full message as JSON. Failures are logged and never block publishing.
    /// </summary>
    public void SaveFull(DataFeedsDiff feedsDiff, FeedsType feedsType, long messageId)
    {
        try
        {
            Directory.CreateDirectory(_dumpPath);
            var filePath = Path.Combine(_dumpPath, $"{feedsType.ToString().ToLowerInvariant()}-full-{messageId}.json");

            var proto = ProtobufConverter.ToProtobuf(feedsDiff);
            using (var writer = new StreamWriter(filePath))
            {
                JsonFormatter.Format(proto, writer);
            }

            _logger.LogInformation("Saved {FeedsType} Full message {MessageId} to {FilePath} ({Size} bytes)",
                feedsType, messageId, filePath, new FileInfo(filePath).Length);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to save {FeedsType} Full message {MessageId} to disk", feedsType, messageId);
        }
    }
}
