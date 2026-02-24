using System.Linq.Expressions;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;
using SportFeedsBridge.Phoenix.Domain.Enums;
using ProtoBuf;

namespace SportFeedsBridge.Phoenix.Models.Feeds.Diff
{
    using SportFeedsBridge.Phoenix.Models.Comparer;

    [DataContract(Name = "DRT"), ProtoContract]
    public class DataResultDiff
        : BaseDiffObject, IDiffMapping<DataResult, DataResultDiff>
    {
        [DataMember, ProtoMember(1)]
        public int ResultId
        {
            get;
            set;
        }

        [DataMember, ProtoMember(2)]
        public short ResultTypeId
        {
            get;
            set;
        }

        [DataMember, ProtoMember(3)]
        public DateTime UpdatedTime
        {
            get;
            set;
        }

        [DataMember, ProtoMember(4)]
        public int ResultValue
        {
            get;
            set;
        }

        [DataMember, ProtoMember(5)]
        public bool IsTeam
        {
            get;
            set;
        }

        public DataResultDiff Instance()
        {
            return new DataResultDiff() { DiffType = DiffType.Equal };
        }

        public DataResultDiff Convert(DataResult from, int deepness, DiffType convertStatus = DiffType.Added)
        {
            DataResultDiff into = Instance();
            if (deepness != 0)
            {
                into.ResultId = from.ResultId;
                into.ResultTypeId = from.ResultTypeId;
                into.UpdatedTime = from.UpdatedTime;
                into.ResultValue = from.ResultValue;
                into.IsTeam = from.IsTeam;
                into.DiffType = convertStatus;
            }
            return into;
        }

        [IgnoreDataMember, JsonIgnore]
        public IEnumerable<Expression<Func<DataResult, object>>> CompareProperties
        {
            get
            {
                yield return x => x.ResultId;
                yield return x => x.UpdatedTime;
                yield return x => x.ResultValue;
            }
        }

        protected override string TypeName
        {
            get { return "DRT"; }
        }
    }
}
