namespace SportFeedsBridge.Configuration;

public class MongoDbSettings
{
    public string ConnectionString { get; set; } = "mongodb://localhost:27017/SportFeeds";
    public string FeedsMessagesCollection { get; set; } = "FeedsMessages";
    public string FixedSnapshotMessagesCollection { get; set; } = "FixedSnapshotMessages";
    public string LiveSnapshotMessagesCollection { get; set; } = "LiveSnapshotMessages";
    public string GridFSBucket { get; set; } = "fs";
}
