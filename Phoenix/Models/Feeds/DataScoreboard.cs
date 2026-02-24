using System.Runtime.Serialization;
using ProtoBuf;

namespace SportFeedsBridge.Phoenix.Models.Feeds
{
    [ProtoContract]
    public class DataScoreboard //TODO:will we need translations?? mabe for the added ones. I will see later. : BaseDataTranslations
    {
        protected bool Equals(DataScoreboard other)
        {
            return IdResultType == other.IdResultType && string.Equals(ResultValue, other.ResultValue);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (IdResultType*397) ^ (ResultValue != null ? ResultValue.GetHashCode() : 0);
            }
        }

        public static bool operator ==(DataScoreboard left, DataScoreboard right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(DataScoreboard left, DataScoreboard right)
        {
            return !Equals(left, right);
        }

        [ProtoMember(1)]
        public int IdResultType { get; set; }

        [ProtoMember(2)]
        public string ResultValue { get; set; }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((DataScoreboard) obj);
        }
    }
}
