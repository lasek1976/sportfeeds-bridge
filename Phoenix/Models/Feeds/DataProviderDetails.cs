using System.Runtime.Serialization;
using ProtoBuf;

namespace SportFeedsBridge.Phoenix.Models.Feeds
{
    [ProtoContract]
    public class DataProviderDetails
    {
        [ProtoMember(1)] public DataProvider Provider { get; set; }

        [ProtoMember(2)] public string ProviderIDEvent { get; set; }

        [ProtoMember(3)] public int ProviderIDSport { get; set; }

        [ProtoMember(4)] public string ProviderIDCategory { get; set; }

        [ProtoMember(5)] public string ProviderIDTournament { get; set; }

        [ProtoMember(7)] public bool HasStatistics { get; set; }

        [ProtoMember(8)] public bool LiveMultiCast { get; set; }

        [ProtoMember(9)] public bool LiveScore { get; set; }

        [ProtoMember(10)] public DataRoundInfo RoundInfo { get; set; }

        [ProtoMember(12)] public int EventKey { get; set; }

        [ProtoMember(13)] public DateTime ProviderDate { get; set; }

        public override bool Equals(object obj)
        {
            if (obj == null)
                return false;
            if (ReferenceEquals(this, obj))
                return true;

            return (obj as DataProviderDetails).Provider == Provider;
        }

        public override int GetHashCode()
        {
            if (Provider != null && Provider.Code != null)
                return Provider.Code.GetHashCode();
            return 0;
        }
    }
}
