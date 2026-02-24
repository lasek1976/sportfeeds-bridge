using SportFeedsBridge.Phoenix.Models.Comparer;

namespace Phoenix.SportFeeds.Application.Models.Comparer
{
    using System.Collections.Concurrent;
    using Microsoft.Extensions.DependencyInjection;

    public class DiffMappingFactory : IDiffMappingFactory
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ICompareFunctionFactory _compareFunctionFactory;
        private readonly ConcurrentDictionary<Type, object> _mappers = new();

        public DiffMappingFactory(IServiceProvider serviceProvider, ICompareFunctionFactory compareFunctionFactory)
        {
            _serviceProvider = serviceProvider;
            _compareFunctionFactory = compareFunctionFactory;
        }

        public DiffMappingCached<T, V> GetMapCached<T, V>() where V : IDiffObject
        {
            var map = _mappers.GetOrAdd(typeof(T), _ =>
            {
                // Resolve the IDiffMapping<T, V> from DI container
                var diffMapping = _serviceProvider.GetRequiredService<IDiffMapping<T, V>>();
                
                // Wrap it in a cached version
                var cachedMapping = new DiffMappingCached<T, V>(diffMapping, _compareFunctionFactory, this);
                
                return cachedMapping;
            });
            
            return (DiffMappingCached<T, V>)map;
        }
    }
}
