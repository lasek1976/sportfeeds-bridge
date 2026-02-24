using System.Runtime.Serialization;
using ProtoBuf;

namespace SportFeedsBridge.Phoenix.Models.Feeds
{
    [ProtoContract]
    public class DataProposalSuperComboClient
    {
        [ProtoMember(1)]
        public int IdClient { get; set; }

        [ProtoMember(2)]
        public string Description { get; set; }

        public override bool Equals(object obj)
        {
            if (obj == null)
                return false;

            if (ReferenceEquals(this, obj))
                return true;

            var proposal = (obj as DataProposalSuperComboClient);

            if (proposal == null)
            {
                return false;
            }

            return proposal.IdClient == IdClient;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return IdClient;
            }
        }
    }
}
