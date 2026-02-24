using System.Runtime.Serialization;
using ProtoBuf;

namespace SportFeedsBridge.Phoenix.Models.Feeds
{
    [ProtoContract]
    public class DataRoundInfo
    {
        [ProtoMember(1)]
        public int Id { get; set; }
        [ProtoMember(2)]
        public string CupRound { get; set; }
        [ProtoMember(3)]
        public string MatchNumber { get; set; }
        [ProtoMember(4)]
        public int Round { get; set; }
        [ProtoMember(5)]
        public string DateInfoValue { get; set; }

        public override bool Equals(object obj)
        {
            if (obj == null)
                return false;
            if (ReferenceEquals(this, obj))
                return true;
            var info = (obj as DataRoundInfo);

            return info.Id == Id
                && string.Equals(info.CupRound, CupRound, StringComparison.InvariantCultureIgnoreCase)
                && string.Equals(info.MatchNumber, MatchNumber, StringComparison.InvariantCultureIgnoreCase)
                && string.Equals(info.DateInfoValue, DateInfoValue, StringComparison.InvariantCultureIgnoreCase)
                && info.Round == Round;
        }

        public override int GetHashCode()
        {
            return Id;
        }

    }
}
