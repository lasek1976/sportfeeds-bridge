using Microsoft.Extensions.Logging;
using SportFeedsBridge.Phoenix.Models.Feeds.Diff;
using System.IO;
using Google.Protobuf;
using Google.Protobuf.WellKnownTypes;
using System.Text.Json;

namespace SportFeedsBridge.Services;

/// <summary>
/// Debug service to save serialized protobuf bytes to file for JavaScript analysis
/// Uses Google.Protobuf for standard proto3 serialization
/// </summary>
public class DebugProtoBufService
{
    private readonly ILogger<DebugProtoBufService> _logger;
    private readonly string _debugPath = Path.Combine(Directory.GetCurrentDirectory(), "debug-protobuf");

    public DebugProtoBufService(ILogger<DebugProtoBufService> logger)
    {
        _logger = logger;
        Directory.CreateDirectory(_debugPath);
    }
    
    /// Save test messages to file for JavaScript analysis
    /// </summary>
    public void SaveTestMessage(DataFeedsDiff feedsDiff)
    {
        try
        {
            // Test 1: Empty message with just root fields (no events)
            var emptyDto = new Sportfeeds.DataFeedsDiff
            {
                CreatedUTCTime = Timestamp.FromDateTime(DateTime.UtcNow),
                DiffType = Sportfeeds.DiffType.Updated
            };

            SaveMessage(emptyDto, "empty-message.bin", "Empty message (no events)");

            // Test 2: Message with just root fields + DifferenceProperties
            var withPropsDto = new Sportfeeds.DataFeedsDiff
            {
                CreatedUTCTime = Timestamp.FromDateTime(DateTime.UtcNow),
                DiffType = Sportfeeds.DiffType.Updated
            };
            withPropsDto.DifferenceProperties.Add(new Sportfeeds.DiffKeyValue
            {
                Key = "test",
                Value = "value"
            });

            SaveMessage(withPropsDto, "with-props-message.bin", "With DifferenceProperties");

            // Test 3: Full message with all events
            var fullDto = ProtobufConverter.ToProtobuf(feedsDiff);
            SaveMessage(fullDto, "full-message.bin", $"Full message ({fullDto.Events?.Count ?? 0} events)");

        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to save test messages");
        }
    }

    private void SaveMessage(Sportfeeds.DataFeedsDiff dto, string filename, string description)
    {
        using var ms = new MemoryStream();
        dto.WriteTo(ms);
        var bytes = ms.ToArray();

        var filePath = Path.Combine(_debugPath, filename);
        File.WriteAllBytes(filePath, bytes);

        _logger.LogInformation("=== SAVED: {Description} ===", description);
        _logger.LogInformation("File: {FilePath}", filePath);
        _logger.LogInformation("Size: {Size} bytes", bytes.Length);
        // Hex dump removed for cleaner console output
        _logger.LogInformation("=====================================\n");
    }

    /// <summary>
    /// Save specific events for sports to JSON for debugging
    /// </summary>
    public void SaveEventsBySport(SportFeedsBridge.Phoenix.Models.Feeds.Diff.DataFeedsDiff feedsDiff)
    {
        try
        {
            // Convert Phoenix model to Google.Protobuf model
            var fullDto = ProtobufConverter.ToProtobuf(feedsDiff);

            foreach (var sportId in fullDto.Events.Select(e => e.IDSport).Distinct().ToList())
            {
                var sportEvents = fullDto.Events.Where(e => e.IDSport == sportId).ToList();

                if (!sportEvents.Any())
                {
                    _logger.LogWarning("No events found for Sport {SportId}", sportId);
                    continue;
                }

                var sportDto = new Sportfeeds.DataFeedsDiff
                {
                    CreatedUTCTime = fullDto.CreatedUTCTime,
                    DiffType = fullDto.DiffType
                };

                // Add events to the protobuf collection
                foreach (var evt in sportEvents)
                {
                    sportDto.Events.Add(evt);
                }
                
                _logger.LogInformation("Sport {SportId} has {Count} events",sportId, sportEvents.Count);

                // var json = JsonFormatter.Default.Format(sportDto);
                // var filename = $"sport-{sportId}-events.json";
                // var filePath = Path.Combine(_debugPath, filename);
                // _logger.LogInformation("Saving json file: {FilePath}", filePath);
                // File.WriteAllText(filePath, json);

            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to save events by sport");
        }
    }
}
