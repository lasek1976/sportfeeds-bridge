using SportFeedsBridge.Phoenix.Domain.Enums;
using ProtoBuf;

namespace SportFeedsBridge.Phoenix.Models.Feeds
{
    [ProtoContract]
    public class DataSelection
        : BaseDataTranslation
    {
        [ProtoMember(1)] public long IDMarket { get; set; }
        [ProtoMember(2)] public int IDMarketType { get; set; }
        [ProtoMember(3)] public long IDSelection { get; set; }
        [ProtoMember(4)] public int IDSelectionType { get; set; }
        [ProtoMember(5)] public int IDTeam { get; set; }
        [ProtoMember(6)] public long IDOdd { get; set; }
        [ProtoMember(7)] public int OnlineCode { get; set; }
        [ProtoMember(8)] public decimal OddValue { get; set; }
        [ProtoMember(9)] public string SelectionName { get; set; }
        [ProtoMember(10)] public decimal? SSpread { get; set; }
        [ProtoMember(11)] public short? ResultOverride { get; set; }
        [ProtoMember(12)] public string ProviderIDTeam { get; set; }
        [ProtoMember(13)] public ProgramStatus SelectionStatus { get; set; }

        public override bool Equals(object obj)
        {
            if (obj == null)
                return false;
            if (ReferenceEquals(this, obj))
                return true;

            return (obj as DataSelection).IDSelection == IDSelection;
        }

        public override int GetHashCode()
        {
            return IDSelection.GetHashCode();
        }
    }
}
