using System.Linq.Expressions;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;
using SportFeedsBridge.Phoenix.Domain.Enums;
using SportFeedsBridge.Phoenix.Models.Comparer;
using ProtoBuf;

namespace SportFeedsBridge.Phoenix.Models.Feeds.Diff
{
    [DataContract(Name = "DF"), ProtoContract]
    public class DataFeedsDiff :
        BaseDiffObject,
        IDiffMapping<DataFeeds, DataFeedsDiff>
    {
        [DataMember(Name="m1"), ProtoMember(1)]
        public List<DataEventDiff> Events
        {
            get;
            set;
        }

        public DataFeedsDiff Instance()
        {
            return new DataFeedsDiff() { DiffType = DiffType.Equal };
        }

        public DataFeedsDiff Convert(DataFeeds from, int deepness, DiffType convertStatus = DiffType.Added)
        {
            DataFeedsDiff into = Instance();
            if (deepness != 0)
            {
                into.Events = ConvertList<DataEvent, DataEventDiff>(from.Events, deepness, convertStatus);
                into.DiffType = convertStatus;
            }
            return into;
        }

        [IgnoreDataMember, JsonIgnore]
        public IEnumerable<Expression<Func<DataFeeds, object>>> CompareProperties
        {
            get
            {
                yield return y => y.Events;
            }
        }

        protected override string TypeName
        {
            get { return "DF"; }
        }
    }
}
