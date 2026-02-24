using Gamenet.Phoenix.Infrastructure.Caching.Extensions;
using Microsoft.Extensions.Logging;
using Phoenix.SportFeeds.Application.Models.Comparer;
using SportFeedsBridge.Phoenix.Domain.Enums;

namespace SportFeedsBridge.Phoenix.Models.Comparer
{
    public class ComparerService<I, O>
        : IComparerService<I, O> where O : IDiffObject
    {
        private readonly ILogger<ComparerService<I, O>> _logger;
        private readonly IDiffMappingFactory _diffMappingFactory;

        // Constructor with dependency injection
        public ComparerService(
            IDiffMappingFactory diffMappingFactory,
            ILogger<ComparerService<I, O>> logger)
        {
            _diffMappingFactory = diffMappingFactory;
            _logger = logger;
        }

        public O Compare(I left, I right)
        {
            return CompareDataObjects<I, O>(left, right, _diffMappingFactory);
        }

        private static void CompareProperty<T, V>(T left, T right, V diffObject, CompareFunction<T> expression)
            where V : IDiffObject
        {
            var oldValue = expression.CompareExpressionCompiled(left);
            var newValue = expression.CompareExpressionCompiled(right);

            if (!Equals(oldValue, newValue))
            {
                diffObject.AddDiffProperty(expression.CompareMember, oldValue);
                diffObject.DiffType = DiffType.Updated;
            }
        }

        internal static IEnumerable<V> ComparePropertiesArray<T, V>(IEnumerable<T> left, IEnumerable<T> right, IDiffMappingFactory diffMappingFactory)
            where V : IDiffObject
        {
            var mapping = diffMappingFactory.GetMapCached<T, V>();

            var leftItems = left ?? Enumerable.Empty<T>();
            var rightItems = right ?? Enumerable.Empty<T>();

            var resultItems = new List<V>();
            var addedItems = rightItems.Except(leftItems).ToArray();
            if (addedItems.Length > 0)
            {
                //if (_logger.IsDebugEnabled)
                //    _logger.DebugFormat("New items added to array: {0}", addedItems.Length);

                resultItems.AddRange(addedItems.Select(x => mapping.Convert(x, -1, DiffType.Added)));
            }

            var removedItems = leftItems.Except(rightItems).ToArray();
            if (removedItems.Length > 0)
            {
                //if (_logger.IsDebugEnabled)
                //    _logger.DebugFormat("Items removed from feeds: {0}", removedItems.Length);

                resultItems.AddRange(removedItems.Select(x => mapping.Convert(x, -1, DiffType.Removed)));
            }

            var updatedItems = leftItems.Join(rightItems, o => o, i => i, (o, i) => new Tuple<T, T>(o, i))
                .ToArray();

            // There is also updated items
            if (updatedItems.Length > 0)
            {
                var updatedItemsDiff = updatedItems.Select(x => CompareDataObjects<T, V>(x.Item1, x.Item2, diffMappingFactory));

                //if (_logger.IsDebugEnabled)
                //    _logger.DebugFormat("Updated items from feeds: {0}", updatedItemsDiff.Count());

                resultItems.AddRange(updatedItemsDiff);
            }

            return resultItems;
        }

        private static V CompareDataObjects<T, V>(T left, T right, IDiffMappingFactory diffMappingFactory) 
            where V : IDiffObject
        {
            var mapping = diffMappingFactory.GetMapCached<T, V>();

            if (ReferenceEquals(left, right))
            {
                //if (_logger.IsDebugEnabled)
                //    _logger.DebugFormat("Data objects are the same: {0}", typeof(T).Name);

                return mapping.Instance();
            }

            if (left == null)
            {
                //if (_logger.IsDebugEnabled)
                //    _logger.DebugFormat("First merge getting all from new object: {0}", typeof(T).Name);

                return mapping.Convert(right, -1, DiffType.Added);
            }

            if (right == null)
            {
                //if (_logger.IsDebugEnabled)
                //    _logger.DebugFormat("No updates avilable: {0}", typeof(T).Name);

                return mapping.Instance();
            }

            var diffObject = mapping.Convert(right, 1, DiffType.Equal);

            if (diffObject != null)
            {
                //if (_logger.IsDebugEnabled)
                //    _logger.DebugFormat("Comparing objects properties: {0}", diffObject.Id);

                mapping.ComparePropertiesPrimitive.Foreach(func => CompareProperty(left, right, diffObject, func));

                //if (_logger.IsDebugEnabled)
                //    _logger.DebugFormat("Comparing array of properties: {0}", diffObject.Id);

                bool isUpdated = mapping.ComparePropertiesArray
                    .Select(func => func.CompareExpressionCompiled(left, right, diffObject, diffMappingFactory))
                    .DefaultIfEmpty(false)
                    .Aggregate((x, y) => x || y);

                diffObject.DiffType = isUpdated ? DiffType.Updated : diffObject.DiffType;
            }

            return diffObject;
        }
    }
}