namespace SportFeedsBridge.Configuration;

public class ProcessingSettings
{
    public string PublishFormat { get; set; } = "ProtoBuf"; // JSON or ProtoBuf
    public int PollingIntervalSeconds { get; set; } = 5;
    public bool ProcessFixed { get; set; } = true;
    public bool ProcessLive { get; set; } = true;
}
