using SportFeedsBridge.Phoenix.Domain.Extensions;
using ProtoBuf;

namespace SportFeedsBridge.Phoenix.Models.Feeds
{
    [ProtoContract, ProtoInclude(6, typeof(DataEventTeam))]
    public class DataTeam : BaseDataTranslation
    {
        [ProtoMember(1)] public int TeamId { get; set; }

        [ProtoMember(2)] public string ProviderTeamId { get; set; }

        [ProtoMember(3)] public string TeamName { get; set; }

        [ProtoMember(4)] public string ProviderTeamName { get; set; }

        [ProtoMember(5)] public int IDTeamNumber { get; set; }

        [ProtoMember(7)] public bool IsGroup { get; set; }

        [ProtoMember(8)] public int? IDSogei { get; set; }

        [ProtoMember(9)] public string FullTeamName { get; set; }

        public override bool Equals(object obj)
        {
            if (obj == null)
                return false;
            if (ReferenceEquals(this, obj))
                return true;

            var team = obj as DataTeam;
            return team.TeamId == TeamId;
        }

        public override int GetHashCode()
        {
            return ProviderTeamId.WithDefault(() => "0").GetHashCode();
        }
    }
}
