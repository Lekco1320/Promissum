using System;
using System.Linq.Expressions;

namespace Lekco.Wpf.Utility.Filter
{
    public abstract class PropertyFilter
    {
        public string? PropertyName { get; set; }
    }

    public abstract class PropertyFilter<T> : PropertyFilter
    {
        public abstract Expression<Func<T, bool>> GetExpression();

        protected virtual MemberExpression GetMember(ParameterExpression param)
        {
            if (string.IsNullOrWhiteSpace(PropertyName))
            {
                throw new ArgumentException("PropertyName cannot be null or empty.", nameof(PropertyName));
            }

            return Expression.PropertyOrField(param, PropertyName);
        }
    }
}
