using System.Runtime.Serialization;
using ProtoBuf;

namespace SportFeedsBridge.Phoenix.Models.Feeds
{
    [ProtoContract]
    public class DataEventTeam
        : DataTeam
    {
        [ProtoMember(1)] public long EventId { get; set; }
    }
}
