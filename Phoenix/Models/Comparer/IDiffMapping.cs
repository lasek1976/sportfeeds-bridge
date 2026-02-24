namespace SportFeedsBridge.Phoenix.Models.Comparer
{
    using System.Linq.Expressions;
    using SportFeedsBridge.Phoenix.Domain.Enums;

    public interface IDiffMapping<T, V>
        where V : IDiffObject
    {
        IEnumerable<Expression<Func<T, object>>> CompareProperties { get; }
        V Instance();
        V Convert(T from, int deepness, DiffType convertStatus = DiffType.Added);
    }
}
