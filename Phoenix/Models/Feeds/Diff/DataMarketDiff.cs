using System.Linq.Expressions;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;
using SportFeedsBridge.Phoenix.Domain.Enums;
using SportFeedsBridge.Phoenix.Models.Comparer;
using ProtoBuf;

namespace SportFeedsBridge.Phoenix.Models.Feeds.Diff
{
    [DataContract(Name = "DM"), ProtoContract]
    public class DataMarketDiff
        : BaseDiffObject, IDiffMapping<DataMarket, DataMarketDiff>
    {
        [DataMember(Name = "m1"), ProtoMember(1)]
        public long IDEvent { get; set; }
        [DataMember(Name = "m2"), ProtoMember(2)]
        public long IDMarket { get; set; }
        [DataMember(Name = "m3"), ProtoMember(3)]
        public int IDMarketType { get; set; }
        [DataMember(Name = "m4"), ProtoMember(4)]
        public decimal Spread { get; set; }
        [DataMember(Name = "m5"), ProtoMember(5)]
        public int GamePlay { get; set; }
        [DataMember(Name = "m6"), ProtoMember(6)]
        public bool Outright { get; set; }
        [DataMember(Name = "m7"), ProtoMember(7)]
        public int MarketWay { get; set; }
        [DataMember(Name = "m9"), ProtoMember(9)]
        public List<DataSelectionDiff> Selections { get; set; }
        [DataMember(Name = "m10"), ProtoMember(10)]
        public Dictionary<string, IEnumerable<DataTranslation>> MarketNameTranslations { get; set; }
        [DataMember(Name = "m11"), ProtoMember(11)]
        public byte SpreadMarketType { get; set; }

        [DataMember(Name = "m12"), ProtoMember(12)]
        public short MarketOrder { get; set; }

        [DataMember(Name = "m13"), ProtoMember(13)]
        public byte ProgramStatus { get; set; }

        [DataMember(Name = "m14"), ProtoMember(14)]
        public short MarketBook { get; set; }

        [DataMember(Name = "m15"), ProtoMember(15)]
        public int AamsIDMarketType { get; set; }

        [DataMember(Name = "m16"), ProtoMember(16)]
        public string AamsMarketName { get; set; }

        [DataMember(Name = "m17"), ProtoMember(17)]
        public string AamsExtraInfo { get; set; }

        [DataMember(Name = "m18"), ProtoMember(18)]
        public int AamsExtraInfoType { get; set; }

        [DataMember(Name = "m19"), ProtoMember(19)]
        public int AamsGamePlay { get; set; }

        [DataMember(Name = "m20"), ProtoMember(20)]
        public int? AamsLinkedMarketId { get; set; }

        [DataMember(Name = "m21"), ProtoMember(21)]
        public string NameOverride { get; set; }
        [DataMember(Name = "m22"), ProtoMember(22)]
        public int? StatisticValue { get; set; }

        private short _book = -1;
        [IgnoreDataMember, ProtoIgnore, JsonIgnore]
        public short Book
        {
            get
            {
                if (_book > -1)
                    return _book;

                if (Selections == null || Selections.Count == 0)
                {
                    _book = 0;
                }
                else
                {
                    decimal bookPercentage = 0;

                    foreach (var selection in this.Selections)
                    {
                        if (selection.OddValue > 0)
                            bookPercentage += 1 / selection.OddValue;
                    }

                    _book = System.Convert.ToInt16(bookPercentage * 100);
                }
                return _book;
            }
        }

        public DataMarketDiff Instance()
        {
            return new DataMarketDiff() { DiffType = DiffType.Equal };
        }

        public DataMarketDiff Convert(DataMarket from, int deepness, DiffType convertStatus = DiffType.Added)
        {
            DataMarketDiff into = Instance();
            if (deepness != 0)
            {
                into.IDEvent = from.IDEvent;
                into.IDMarket = from.IDMarket;
                into.IDMarketType = from.IDMarketType;
                into.Spread = from.Spread;
                into.GamePlay = from.GamePlay;
                into.Outright = from.Outright;
                into.MarketWay = from.MarketWay;
                into.MarketNameTranslations = from.TranslationsDictionary;
                into.SpreadMarketType = from.SpreadMarketType;
                into.MarketOrder = from.MarketOrder;
                into.ProgramStatus = from.ProgramStatus;
                into.MarketBook = from.MarketBook;
                into.Selections = ConvertList<DataSelection, DataSelectionDiff>(from.Selections, deepness, convertStatus);
                into.DiffType = convertStatus;
                into.AamsIDMarketType = from.AamsIDMarketType;
                into.AamsMarketName = from.AamsMarketName;
                into.AamsExtraInfo = from.AamsExtraInfo;
                into.AamsExtraInfoType = from.AamsExtraInfoType;
                into.AamsGamePlay = from.AamsGamePlay;
                into.AamsLinkedMarketId = from.AamsLinkedMarketId;
                into.NameOverride = from.NameOverride;
                into.StatisticValue = from.StatisticValue;
            }
            return into;
        }

        [IgnoreDataMember, JsonIgnore]
        public IEnumerable<Expression<Func<DataMarket, object>>> CompareProperties
        {
            get
            {
                //yield return x => x.IDEvent;
                //yield return x => x.IDMarket;
                //yield return x => x.IDMarketType;
                //yield return x => x.Spread;
                yield return x => x.GamePlay;
                yield return x => x.ProgramStatus;
                //yield return x => x.Outright;
                //yield return x => x.MarketWay;
                //yield return x => x.SpreadMarketType;
                //yield return x => x.MarketOrder;
                yield return x => x.MarketBook;
                yield return y => y.Selections;
            }
        }

        protected override string TypeName
        {
            get { return "DM"; }
        }
    }
}
