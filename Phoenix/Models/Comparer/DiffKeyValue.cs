using System.Runtime.Serialization;
using ProtoBuf;

namespace SportFeedsBridge.Phoenix.Models.Comparer
{
    [DataContract(Name = "DKV"), ProtoContract()]
    public class DiffKeyValue
    {
        [DataMember(Name = "k"), ProtoMember(1)]
        public string Key { get; set; }
        [DataMember(Name = "v"),ProtoMember(2)]
        public string Value{ get; set; }
    }
}
