using SportFeedsBridge.Phoenix.Models.Comparer;

namespace Phoenix.SportFeeds.Application.Models.Comparer;

using System.Linq.Expressions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

public class CompareFunctionFactory : ICompareFunctionFactory
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILoggerFactory _loggerFactory;
    
    public CompareFunctionFactory(
        IServiceProvider serviceProvider, 
        ILoggerFactory loggerFactory)
    {
        _serviceProvider = serviceProvider;
        _loggerFactory = loggerFactory;
    }
    
    public CompareFunction<T> CreateFunction<T>(Expression<Func<T, object>> expression)
    {
        var logger = _loggerFactory.CreateLogger<CompareFunction<T>>();
        return new CompareFunction<T>(expression, logger);
    }
    
    public CompareArrayFunction<T, V> CreateArrayFunction<T, V>(
        Expression<Func<T, object>> expression) 
        where V : IDiffObject
    {
        var logger = _loggerFactory.CreateLogger<CompareArrayFunction<T, V>>();
        
        // Lazy resolution: Get IDiffMappingFactory from service provider when needed
        var diffMappingFactory = _serviceProvider.GetRequiredService<IDiffMappingFactory>();
        
        return new CompareArrayFunction<T, V>(expression, diffMappingFactory, logger);
    }
}