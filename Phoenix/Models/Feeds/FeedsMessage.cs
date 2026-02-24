using SportFeedsBridge.Phoenix.Domain.Enums;

namespace SportFeedsBridge.Phoenix.Models.Feeds;

public class FeedsMessage
{
    public long MessageId { get; set; }

    public object Body { get; set; }

    public DateTime CreatedTime { get; set; }

    public DateTime ExpirationTime { get; set; }

    public MessageFormat Format { get; set; }

    public long MessageKbSize { get; set; }

    public string DiffType { get; set; }
}
