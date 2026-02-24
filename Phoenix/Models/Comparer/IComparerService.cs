namespace SportFeedsBridge.Phoenix.Models.Comparer
{
    public interface IComparerService<T, V> where V : IDiffObject
    {
        V Compare(T left, T right);
    }
}
