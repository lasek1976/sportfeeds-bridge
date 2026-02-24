using System.Linq.Expressions;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;
using SportFeedsBridge.Phoenix.Domain.Enums;
using SportFeedsBridge.Phoenix.Models.Comparer;
using ProtoBuf;

namespace SportFeedsBridge.Phoenix.Models.Feeds.Diff
{
    [DataContract(Name = "DT"), ProtoContract]
    public class DataTeamDiff
        : BaseDiffObject, IDiffMapping<DataTeam, DataTeamDiff>
    {
        private Dictionary<string, IEnumerable<DataTranslation>> _teamTranslationDictionary = new Dictionary<string, IEnumerable<DataTranslation>>();

        [DataMember(Name = "m1"), ProtoMember(1)]
        public int TeamId { get; set; }
        [DataMember(Name = "m2"), ProtoMember(2)]
        public string ProviderTeamId { get; set; }
        [DataMember(Name = "m3"), ProtoMember(3)]
        public string TeamName { get; set; }
        [DataMember(Name = "m4"), ProtoMember(4)]
        public string ProviderTeamName { get; set; }
        [DataMember(Name = "m5"), ProtoMember(5)]
        public int IdTeamNumber { get;  set;}
        [DataMember(Name = "m7"), ProtoMember(7)]
        public bool IsGroup { get; set; }
        [DataMember(Name = "m8"), ProtoMember(8)]
        public int? IDSogei { get; set; }

        [DataMember(Name = "m9"), ProtoMember(9)]
        public Dictionary<string, IEnumerable<DataTranslation>> TeamTranslationDictionary
        {
                get { return _teamTranslationDictionary; }
                set { _teamTranslationDictionary = value; }
        }

        [DataMember(Name = "m10"), ProtoMember(10)]
        public string FullTeamName { get; set; }

        public DataTeamDiff Instance()
        {
            return new DataTeamDiff() { DiffType = DiffType.Equal };
        }

        public DataTeamDiff Convert(DataTeam from, int deepness, DiffType convertStatus = DiffType.Added)
        {
            DataTeamDiff into = Instance();
            if (deepness != 0)
            {
                into.TeamId = from.TeamId;
                into.ProviderTeamId = from.ProviderTeamId;
                into.TeamName = from.TeamName;
                into.ProviderTeamName = from.ProviderTeamName;
                into.IdTeamNumber = from.IDTeamNumber;
                into.TeamTranslationDictionary = from.TranslationsDictionary;
                into.DiffType = convertStatus;
                into.IsGroup = from.IsGroup;
                into.IDSogei = from.IDSogei;
                into.FullTeamName = from.FullTeamName;
            }
            return into;
        }

        [IgnoreDataMember, JsonIgnore]
        public IEnumerable<Expression<Func<DataTeam, object>>> CompareProperties
        {
            get
            {
                //yield return x => x.TeamId;
                //yield return x => x.ProviderTeamId;
                yield return x => x.TeamName;
                yield return x => x.ProviderTeamName;
                //yield return x => x.IDTeamNumber;
                yield return x => x.IDSogei;
                yield return x => x.FullTeamName;
            }
        }

        protected override string TypeName
        {
            get { return "DT"; }
        }

        public override bool IsRelated
        {
            get
            {
                return true;
            }
        }
    }
}
