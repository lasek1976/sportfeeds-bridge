using System.Linq.Expressions;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;
using SportFeedsBridge.Phoenix.Domain.Enums;
using ProtoBuf;

namespace SportFeedsBridge.Phoenix.Models.Feeds.Diff
{
    using SportFeedsBridge.Phoenix.Models.Comparer;

    [DataContract(Name = "DS"), ProtoContract]
    public class DataStreamingDiff :
        BaseDiffObject, IDiffMapping<DataStreaming, DataStreamingDiff>
    {
        [DataMember(Name = "m1"), ProtoMember(1)]
        public string StreamProvider { get; set; }

        [DataMember(Name = "m2"), ProtoMember(2)]
        public string StreamID { get; set; }

        public DataStreamingDiff Instance()
        {
            return new DataStreamingDiff() { DiffType = DiffType.Equal };
        }

        public DataStreamingDiff Convert(DataStreaming from, int deepness, DiffType convertStatus = DiffType.Added)
        {
            DataStreamingDiff into = Instance();
            if (deepness != 0)
            {
                into.StreamProvider = from.StreamProvider;
                into.StreamID = from.StreamID;
            }

            return into;
        }

        [IgnoreDataMember, JsonIgnore]
        public IEnumerable<Expression<Func<DataStreaming, object>>> CompareProperties
        {
            get
            {
                yield return x => x.StreamProvider;
                yield return x => x.StreamID;
            }
        }

        protected override string TypeName
        {
            get { return "DS"; }
        }
    }
}
