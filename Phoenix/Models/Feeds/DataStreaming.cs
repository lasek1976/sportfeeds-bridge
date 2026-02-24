using System.Runtime.Serialization;
using ProtoBuf;

namespace SportFeedsBridge.Phoenix.Models.Feeds
{
    [ProtoContract]
    public class DataStreaming
    {
        [ProtoMember(1)] public string StreamProvider { get; set; }

        [ProtoMember(2)] public string StreamID { get; set; }
    }
}
