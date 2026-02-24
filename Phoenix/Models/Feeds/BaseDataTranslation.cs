using ProtoBuf;

namespace SportFeedsBridge.Phoenix.Models.Feeds;

[ProtoContract]
public abstract class BaseDataTranslation
{
    [ProtoIgnore]
    private Dictionary<string, IEnumerable<DataTranslation>> _translationsDictionary =
        new Dictionary<string, IEnumerable<DataTranslation>>();

    public Dictionary<string, IEnumerable<DataTranslation>> TranslationsDictionary
    {
        get { return _translationsDictionary; }
        set { _translationsDictionary = value; }
    }
}
