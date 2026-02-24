namespace SportFeedsBridge.Phoenix.Models.Comparer;

using SportFeedsBridge.Phoenix.Domain.Enums;

public interface IDiffObject
{
    DateTime CreatedUTCTime { get; }
    string Id { get; }
    DiffType DiffType { get; set; }
    bool IsRelated { get;}
    void AddDiffProperty(string propertyName, object oldValue);

    IDiffMappingFactory DiffMappingFactory { get; set; }
}
