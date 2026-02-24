namespace Phoenix.SportFeeds.Application.Models.Comparer
{
    using System.Linq.Expressions;
    using Microsoft.Extensions.Logging;

    public class CompareFunction<T>
    {
        private readonly ILogger<CompareFunction<T>> _logger;
        public CompareFunction(Expression<Func<T, object>> expression,
            ILogger<CompareFunction<T>>  logger)
        {
            _logger = logger;
            CompareExpression = expression;
            CompileExpression();
        }

        private void CompileExpression()
        {
            var ue = CompareExpression.Body as UnaryExpression;
            MemberExpression me = ue != null ? ue.Operand as MemberExpression : CompareExpression.Body as MemberExpression;
            if (me == null)
            {
                var exception = new ArgumentException("Only member expression allowed for compare expression");
                _logger.LogError(exception,"Only member expression allowed for compare expression");
                throw exception;
            }

            CompareExpressionCompiled = CompareExpression.Compile();
            CompareMember = me.Member.Name;
        }

        public Expression<Func<T, object>> CompareExpression
        {
            get;
            private set;
        }

        public Func<T, object> CompareExpressionCompiled
        {
            get;
            private set;
        }

        public string CompareMember
        {
            get;
            private set;
        }
    }
}
