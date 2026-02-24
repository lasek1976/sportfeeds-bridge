using MongoDB.Bson.Serialization.Attributes;
using SportFeedsBridge.Phoenix.Domain.Enums;

namespace SportFeedsBridge.Phoenix.Models.Feeds;

public class SnapshotMessage
{
    public long SnapshotID { get; set; }
    public long MessageId { get; set; }
    public DateTime ExpirationTime { get; set; }
    public DateTime CreatedTime { get; set; }
    public FeedsType FeedsType { get; set; }
}
