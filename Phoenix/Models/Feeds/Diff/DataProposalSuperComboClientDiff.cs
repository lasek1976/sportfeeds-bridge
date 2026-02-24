using System.Linq.Expressions;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;
using SportFeedsBridge.Phoenix.Domain.Enums;
using ProtoBuf;

namespace SportFeedsBridge.Phoenix.Models.Feeds.Diff
{
    using SportFeedsBridge.Phoenix.Models.Comparer;

    [DataContract(Name = "PSC"), ProtoContract]
    public class DataProposalSuperComboClientDiff
        : BaseDiffObject, IDiffMapping<DataProposalSuperComboClient, DataProposalSuperComboClientDiff>
    {
        [DataMember(Name = "m1"), ProtoMember(1)]
        public int IdClient { get; set; }
        [DataMember(Name = "m2"), ProtoMember(2)]
        public string Description { get; set; }
        public DataProposalSuperComboClientDiff Instance()
        {
            return new DataProposalSuperComboClientDiff() { DiffType = DiffType.Equal };
        }

        public DataProposalSuperComboClientDiff Convert(DataProposalSuperComboClient from, int deepness, DiffType convertStatus = DiffType.Added)
        {
            DataProposalSuperComboClientDiff into = Instance();
            if (deepness != 0)
            {
                into.IdClient = from.IdClient;
                into.Description = from.Description;
                into.DiffType = convertStatus;
            }
            return into;
        }

        [IgnoreDataMember, JsonIgnore]
        public IEnumerable<Expression<Func<DataProposalSuperComboClient, object>>> CompareProperties
        {
            get
            {
                yield return x => x.IdClient;
                yield return x => x.Description;
            }
        }

        protected override string TypeName
        {
            get { return "PSC"; }
        }
    }
}
