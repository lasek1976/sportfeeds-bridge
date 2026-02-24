using System.Linq.Expressions;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;
using SportFeedsBridge.Phoenix.Domain.Enums;
using SportFeedsBridge.Phoenix.Models.Comparer;
using ProtoBuf;

namespace SportFeedsBridge.Phoenix.Models.Feeds.Diff
{
    [DataContract(Name = "DS"), ProtoContract]
    public class DataSelectionDiff
        : BaseDiffObject, IDiffMapping<DataSelection, DataSelectionDiff>
    {
        [DataMember(Name = "m1"), ProtoMember(1)]
        public long IDMarket { get; set; }
        [DataMember(Name = "m2"), ProtoMember(2)]
        public long IDSelection { get; set; }
        [DataMember(Name = "m3"), ProtoMember(3)]
        public int IDSelectionType { get; set; }
        [DataMember(Name = "m4"), ProtoMember(4)]
        public int IDTeam { get; set; }
        [DataMember(Name = "m5"), ProtoMember(5)]
        public string SelectionName { get; set; }
        [DataMember(Name = "m6"), ProtoMember(6)]
        public Dictionary<string, IEnumerable<DataTranslation>> SelectionNameTranslations { get; set; }
        [DataMember(Name = "m7"), ProtoMember(7)]
        public long IDOdd { get; set; }
        [DataMember(Name = "m8"), ProtoMember(8)]
        public decimal OddValue { get; set; }
        [DataMember(Name = "m9"), ProtoMember(9)]
        public int OnlineCode { get; set; }
        [DataMember(Name = "m10"), ProtoMember(10)]
        public decimal? SSpread { get; set; }
        [DataMember(Name = "m11"), ProtoMember(11)]
        public short? ResultOverride { get; set; }
        [DataMember(Name = "m12"), ProtoMember(12)]
        public string ProviderIdTeam { get; set; }
        [DataMember(Name = "m13"), ProtoMember(13)]
        public ProgramStatus SelectionStatus { get; set; }

        public DataSelectionDiff Instance()
        {
            return new DataSelectionDiff() { DiffType = DiffType.Equal };
        }

        public DataSelectionDiff Convert(DataSelection from, int deepness, DiffType convertStatus = DiffType.Added)
        {
            DataSelectionDiff into = Instance();
            if (deepness != 0)
            {
                into.IDMarket = from.IDMarket;
                into.IDSelection = from.IDSelection;
                into.SelectionName = from.SelectionName;
                into.IDSelectionType = from.IDSelectionType;
                into.IDTeam = from.IDTeam;
                into.IDOdd = from.IDOdd;
                into.OddValue = from.OddValue;
                into.OnlineCode = from.OnlineCode;
                into.SelectionNameTranslations = from.TranslationsDictionary;
                into.SSpread = from.SSpread;
                into.ResultOverride = from.ResultOverride;
                into.ProviderIdTeam = from.ProviderIDTeam;
                into.DiffType = convertStatus;
                into.SelectionStatus = from.SelectionStatus;

                //check if selection must forcibly closing
                if (new[] {ProgramStatus.NotStartedRetired}.Contains(from.SelectionStatus))
                {
                    into.IDOdd = 0;
                    into.OddValue = 0;
                }
            }
            return into;
        }

        [IgnoreDataMember, JsonIgnore]
        public IEnumerable<Expression<Func<DataSelection, object>>> CompareProperties
        {
            get
            {
                //yield return x => x.IDMarket;
                //yield return x => x.IDSelection;
                //yield return x => x.IDSelectionType;
                yield return x => x.IDOdd;
                yield return x => x.OddValue;
                //yield return x => x.IDTeam;
                yield return x => x.OnlineCode;
                yield return x => x.SSpread;
                yield return x => x.ResultOverride;
            }
        }

        protected override string TypeName
        {
            get { return "DS"; }
        }
    }
}
