using ProtoBuf;

namespace SportFeedsBridge.Phoenix.Models.Feeds
{
    [ProtoContract]
    public class DataMarket
        : BaseDataTranslation
    {
        [ProtoMember(1)] public long IDEvent { get; set; }

        [ProtoMember(2)] public long IDMarket { get; set; }

        [ProtoMember(3)] public int IDMarketType { get; set; }

        [ProtoMember(4)] public int MarketWay { get; set; }

        [ProtoMember(5)] public decimal Spread { get; set; }

        [ProtoMember(6)] public int GamePlay { get; set; }

        [ProtoMember(7)] public bool Outright { get; set; }

        [ProtoMember(9)] public List<DataSelection> Selections { get; set; } = [];

        [ProtoMember(10)] public string MarketName { get; set; }

        [ProtoMember(11)] public byte SpreadMarketType { get; set; }

        [ProtoMember(12)] public short MarketOrder { get; set; }

        [ProtoMember(13)] public byte ProgramStatus { get; set; }

        [ProtoMember(14)] public short MarketBook { get; set; }

        [ProtoMember(15)] public int AamsIDMarketType { get; set; }

        [ProtoMember(16)] public string AamsMarketName { get; set; }

        [ProtoMember(17)] public string AamsExtraInfo { get; set; }

        [ProtoMember(18)] public int AamsExtraInfoType { get; set; }

        [ProtoMember(19)] public int AamsGamePlay { get; set; }

        [ProtoMember(20)] public int? AamsLinkedMarketId { get; set; }

        [ProtoMember(21)] public string NameOverride { get; set; }
        [ProtoMember(22)] public int? StatisticValue { get; set; }

        public override bool Equals(object obj)
        {
            if (obj == null)
                return false;
            if (ReferenceEquals(this, obj))
                return true;

            var market = (obj as DataMarket);

            if (market == null)
            {
                return false;
            }

            return market.IDMarket == IDMarket && market.Spread == Spread;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (int)IDMarket;
            }
        }
    }
}
