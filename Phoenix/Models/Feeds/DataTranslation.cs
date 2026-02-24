using System.Runtime.Serialization;
using ProtoBuf;

namespace SportFeedsBridge.Phoenix.Models.Feeds
{
    [ProtoContract]
    public class DataTranslation
    {
        [ProtoMember(1)] public string Language { get; set; }

        [ProtoMember(2)] public string Value { get; set; }

        [ProtoMember(3)] public DataProvider Provider { get; set; }

        [ProtoMember(4)] public string ProviderId { get; set; }

        public override bool Equals(object obj)
        {
            if (obj == null)
                return false;
            if (ReferenceEquals(this, obj))
                return true;

            var translation = obj as DataTranslation;
            return translation?.Provider == Provider && translation.Language == Language;
        }

        public override int GetHashCode()
        {
            if (Provider != null && Provider.Code != null)
                return Tuple.Create(Provider.Code, Language).GetHashCode();
            return Language.GetHashCode();
        }
    }
}
