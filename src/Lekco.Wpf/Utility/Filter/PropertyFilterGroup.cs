using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace Lekco.Wpf.Utility.Filter
{
    public class PropertyFilterGroup<T> : PropertyFilter<T>, IFilter<PropertyFilter<T>>
    {
        public LogicalOperator Mode { get; set; }

        public IList<PropertyFilter<T>> Children { get; set; } = [];

        public override Expression<Func<T, bool>> GetExpression()
        {
            var childExprs = Children.Select(f => f.GetExpression());

            return Mode == LogicalOperator.And
                 ? ExpressionCombiner.AndAlsoAll(childExprs)
                 : ExpressionCombiner.OrElseAll(childExprs);
        }
    }

    public enum LogicalOperator
    {
        And,
        Or
    }
}
