using System.Linq.Expressions;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;
using SportFeedsBridge.Phoenix.Domain.Enums;
using SportFeedsBridge.Phoenix.Models.Comparer;
using ProtoBuf;

namespace SportFeedsBridge.Phoenix.Models.Feeds.Diff
{
    [DataContract(Name = "DSB"), ProtoContract]
    public class DataScoreBoardDiff : BaseDiffObject, IDiffMapping<DataScoreboard, DataScoreBoardDiff>
    {
        [DataMember(Name="m1"), ProtoMember(1)]
        public int IdResultType { get; set; }
        [DataMember(Name = "m2"), ProtoMember(2)]
        public string ResultValue { get; set; }


        protected override string TypeName
        {
            get { return "DSB"; }
        }

        [IgnoreDataMember, JsonIgnore]
        public IEnumerable<Expression<Func<DataScoreboard, object>>> CompareProperties
        {
            get
            {
                yield return x => x.IdResultType;
                yield return x => x.ResultValue;
            }
        }

        public DataScoreBoardDiff Instance()
        {
            return new DataScoreBoardDiff {DiffType = DiffType.Equal};
        }

        public DataScoreBoardDiff Convert(DataScoreboard from, int deepness, DiffType convertStatus = DiffType.Added)
        {
            DataScoreBoardDiff into = Instance();
            if (deepness != 0)
            {
                into.IdResultType = from.IdResultType;
                into.ResultValue = from.ResultValue;
                into.DiffType = convertStatus;
            }
            return into;
        }
    }
}
