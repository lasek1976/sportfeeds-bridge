using System.Runtime.Serialization;
using ProtoBuf;

namespace SportFeedsBridge.Phoenix.Models.Feeds
{
    [ProtoContract]
    public class DataResult
    {
        [ProtoMember(1)] public int ResultId { get; set; }

        [ProtoMember(2)] public short ResultTypeId { get; set; }

        [ProtoMember(3)] public DateTime UpdatedTime { get; set; }

        [ProtoMember(4)] public int ResultValue { get; set; }

        [ProtoMember(5)] public bool IsTeam { get; set; }

        public override bool Equals(object obj)
        {
            if (obj == null || GetType() != obj.GetType())
            {
                return false;
            }

            return ResultId == (obj as DataResult).ResultId;
        }

        public override int GetHashCode()
        {
            return ResultId;
        }
    }
}
