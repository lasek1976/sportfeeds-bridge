namespace SportFeedsBridge.Phoenix.Models.Comparer;

using System.Runtime.Serialization;
using Gamenet.Phoenix.Infrastructure.Caching.Extensions;
using SportFeedsBridge.Phoenix.Models.Feeds.Diff;
using SportFeedsBridge.Phoenix.Domain.Enums;
using ProtoBuf;

[DataContract(Name = "BDO"), ProtoContract(IgnoreListHandling = true)]
[ProtoInclude(500, typeof(DataEventDiff))]
[ProtoInclude(501, typeof(DataMarketDiff))]
[ProtoInclude(502, typeof(DataSelectionDiff))]
[ProtoInclude(503, typeof(DataProviderDetailsDiff))]
[ProtoInclude(504, typeof(DataFeedsDiff))]
[ProtoInclude(505, typeof(DataScoreBoardDiff))]
[ProtoInclude(506, typeof(DataResultDiff))]
[ProtoInclude(507, typeof(DataTeamDiff))]
[ProtoInclude(508, typeof(DataStreamingDiff))]
[ProtoInclude(509, typeof(DataProposalSuperComboClientDiff))]
public abstract class BaseDiffObject
    : IDiffObject
{
    [DataMember(Name = "i3"), ProtoMember(103)]
    public List<DiffKeyValue> DifferenceProperties = new List<DiffKeyValue>();

    //this is much faster than DateTime.Now  so don't change it
    [DataMember(Name = "i1"), ProtoMember(101)]
    private readonly DateTime _createdUtctime = DateTime.UtcNow;

    [IgnoreDataMember, ProtoIgnore]
    public IDiffMappingFactory DiffMappingFactory { get; set; }

    [IgnoreDataMember]
    public DateTime CreatedUTCTime
    {
        get { return _createdUtctime; }
    }

    [DataMember(Name = "i2"), ProtoMember(102)]
    public DiffType DiffType { get; set; }

    protected abstract string TypeName { get; }

    [IgnoreDataMember, ProtoIgnore]
    public virtual bool IsRelated
    {
        get { return false; }
    }

    protected BaseDiffObject()
    {
    }

    public void AddDiffProperty(string propertyName, object oldValue)
    {
        if (propertyName.IsNullOrEmpty() || oldValue == null)
        {
            return;
        }

        var value = new DiffKeyValue { Key = propertyName, Value = string.Format("{0}", oldValue) };

        DifferenceProperties.Add(value);
    }

    [IgnoreDataMember, ProtoIgnore]
    public string Id
    {
        get { return string.Format("{0}:{1:ddMMyyy-HHmmss}", TypeName, CreatedUTCTime); }
    }

    protected List<V> ConvertList<T, V>(List<T> input, int deepness, DiffType convertStatus = DiffType.Added)
        where V : IDiffMapping<T, V>, IDiffObject, new()
        where T : class
    {
        var mapper = DiffMappingFactory.GetMapCached<T, V>();
        return (input != null && deepness != 1
            ? input
                .Select(x => mapper.Convert(x, deepness - 1, convertStatus))
                .Where(x => x.DiffType != DiffType.Equal)
            : Enumerable.Empty<V>()).ToList();
    }

    protected V Convert<T, V>(T input, int deepness, DiffType convertStatus = DiffType.Added)
        where V : IDiffMapping<T, V>, IDiffObject, new()
        where T : class
    {
        //var mapper = DiffMappingFactory.Instance.GetMapCached<T, V>();
        var mapper = DiffMappingFactory.GetMapCached<T, V>();
        return input != null && deepness != 1
            ? mapper.Convert(input, deepness - 1, convertStatus)
            : default(V);
    }
}
