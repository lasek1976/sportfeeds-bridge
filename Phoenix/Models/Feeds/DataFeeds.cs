using System.Runtime.Serialization;
using ProtoBuf;

namespace SportFeedsBridge.Phoenix.Models.Feeds
{
    [ProtoContract]
    public class DataFeeds
    {
        [ProtoMember(1)] public List<DataEvent> Events { get; set; }
        [ProtoMember(2)] public DateTime CreateTime { get; set; }
        [ProtoMember(3)] public DateTime? MaxChangingTimeRetrieved { get; set; }
    }
}
