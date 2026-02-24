using Phoenix.SportFeeds.Application.Models.Comparer;

namespace SportFeedsBridge.Phoenix.Models.Comparer
{
    public interface IDiffMappingFactory
    {
        DiffMappingCached<T, V> GetMapCached<T, V>() where V : IDiffObject;
    }
}
