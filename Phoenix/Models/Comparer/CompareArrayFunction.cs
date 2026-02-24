using System.Linq.Expressions;
using Microsoft.Extensions.Logging;
using Phoenix.SportFeeds.Application.Models.Comparer;
using SportFeedsBridge.Phoenix.Domain.Enums;

namespace SportFeedsBridge.Phoenix.Models.Comparer
{
    public class CompareArrayFunction<T, V>
        where V : IDiffObject
    {
        private readonly ILogger<CompareArrayFunction<T, V>> _logger;
        private readonly IDiffMappingFactory _diffMappingFactory;
        
        public CompareArrayFunction(Expression<Func<T, object>> expression,
            IDiffMappingFactory diffMappingFactory,
            ILogger<CompareArrayFunction<T, V>> logger)
        {
            _logger = logger;
            _diffMappingFactory = diffMappingFactory;
            CompareExpression = expression;
            CompileExpression();
        }

        private void CompileExpression()
        {
            var arrayPropertMemebr = (CompareExpression.Body as MemberExpression).Member;
            var propertyName = arrayPropertMemebr.Name;

            var diffObjectParam = Expression.Parameter(typeof(V), "diffObject");
            var diffObjectArrayProperty = Expression.Property(diffObjectParam, propertyName);

            var leftparam = Expression.Parameter(typeof(T), "left");
            var rightparam = Expression.Parameter(typeof(T), "right");
            var factoryParam = Expression.Parameter(typeof(IDiffMappingFactory), "factory");

            var compareArrayResult = Expression.Variable(diffObjectArrayProperty.Type.GetInterface("IEnumerable`1"), "result");
            var compareArrayItem = Expression.Parameter(diffObjectArrayProperty.Type.GenericTypeArguments[0], "item");
            var isUpdatedParam = Expression.Variable(typeof(bool), "isUpdated");

            var returnTarget = Expression.Label(typeof(bool), "return");

            var compareLambda = Expression.Lambda(
                Expression.NotEqual(
                    Expression.Property(compareArrayItem, "DiffType"), Expression.Constant(DiffType.Equal)
                    ),
                    compareArrayItem
                );

            try
            {
                var block = Expression.Block(
                    new[] { compareArrayResult, isUpdatedParam },
                    Expression.Assign(compareArrayResult,
                        Expression.Call(
                            typeof(ComparerService<T, V>),
                            "ComparePropertiesArray",
                            new Type[]
                            {
                                CompareExpression.Body.Type.GenericTypeArguments[0],
                                diffObjectArrayProperty.Type.GenericTypeArguments[0]
                            },
                            Expression.Property(leftparam, propertyName), 
                            Expression.Property(rightparam, propertyName),
                            factoryParam)
                    ),
                    Expression.Call(diffObjectArrayProperty, "AddRange", null, compareArrayResult),
                    Expression.Assign(isUpdatedParam,
                        Expression.Call(
                            typeof(Enumerable),
                            "Any",
                            new Type[] { diffObjectArrayProperty.Type.GenericTypeArguments[0] },
                            compareArrayResult, compareLambda)
                    ),
                    Expression.Label(returnTarget, isUpdatedParam)
                );
                
                CompareExpressionCompiled = Expression.Lambda<Func<T, T, V, IDiffMappingFactory, bool>>(
                    block, leftparam, rightparam, diffObjectParam, factoryParam).Compile();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.ToString());
            }
        }

        public Expression<Func<T, object>> CompareExpression
        {
            get;
            private set;
        }

        public Func<T, T, V, IDiffMappingFactory, bool> CompareExpressionCompiled
        {
            get;
            private set;
        }
    }
}
