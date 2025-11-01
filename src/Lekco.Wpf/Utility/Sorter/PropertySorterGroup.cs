using System.Collections.Generic;
using System.Linq.Expressions;

namespace Lekco.Wpf.Utility.Sorter
{
    public class PropertySorterGroup<T>
    {
        public IList<PropertySorter<T>> Sorters { get; set; } = [];

        public virtual Expression AddQueryExpression(Expression exp)
        {
            bool first = true;
            Expression thisExp = exp;
            foreach (var sorter in Sorters)
            {
                thisExp = sorter.AddQueryExpression(thisExp, first);
                first = false;
            }
            return thisExp;
        }
    }
}
