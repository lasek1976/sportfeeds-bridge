using SportFeedsBridge.Phoenix.Domain.Enums;
using SportFeedsBridge.Phoenix.Models.Comparer;

namespace Phoenix.SportFeeds.Application.Models.Comparer
{
    using System.Linq.Expressions;

    public class DiffMappingCached<T, V>
            : IDiffMapping<T, V> where V : IDiffObject
    {
        private readonly IDiffMapping<T, V> _originalMap;
        private readonly IDiffMappingFactory _diffMappingFactory;
        
        public DiffMappingCached(
            IDiffMapping<T, V> originalMap, 
            ICompareFunctionFactory compareFunctionFactory, 
            IDiffMappingFactory diffMappingFactory)
        {
            _originalMap = originalMap;
            _diffMappingFactory = diffMappingFactory;
            
            if (_originalMap is BaseDiffObject baseDiffObject)
            {
                baseDiffObject.DiffMappingFactory = _diffMappingFactory;  
            }
            
            ComparePropertiesPrimitive = _originalMap.CompareProperties
                .Where(x => x.Body.Type.Name != "List`1")
                .Select(x => compareFunctionFactory.CreateFunction<T>(x))
                .ToArray();
            ComparePropertiesArray = _originalMap
                .CompareProperties.Where(x => x.Body.Type.Name == "List`1")
                .Select(x => compareFunctionFactory.CreateArrayFunction<T,V>(x))
                .ToArray();
        }

        public IEnumerable<CompareFunction<T>> ComparePropertiesPrimitive
        {
            get;
            private set;
        }

        public IEnumerable<CompareArrayFunction<T, V>> ComparePropertiesArray
        {
            get;
            private set;
        }

        public V Instance()
        {
            var instance = _originalMap.Instance();
            if (instance != null)
            {
                instance.DiffMappingFactory = _diffMappingFactory;
            }
            return instance;
        }

        public V Convert(T from, int deepness, DiffType convertStatus = DiffType.Added)
        {
            var converted = _originalMap.Convert(from, deepness, convertStatus);
            if (converted != null)
            {
                converted.DiffMappingFactory = _diffMappingFactory;
            }
            return converted;
        }

        public IEnumerable<Expression<Func<T, object>>> CompareProperties
        {
            get { return _originalMap.CompareProperties; }
        }
    }
}
