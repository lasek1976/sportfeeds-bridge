using System.Linq.Expressions;
using Phoenix.SportFeeds.Application.Models.Comparer;
using SportFeedsBridge.Phoenix.Models.Comparer;

public interface ICompareFunctionFactory
{
    CompareFunction<T> CreateFunction<T>(Expression<Func<T, object>> expression);
    CompareArrayFunction<T, V> CreateArrayFunction<T, V>(Expression<Func<T, object>> expression) where V : IDiffObject;
}