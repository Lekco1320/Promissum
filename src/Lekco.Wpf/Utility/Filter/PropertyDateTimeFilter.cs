using System;
using System.Linq.Expressions;

namespace Lekco.Wpf.Utility.Filter
{
    public class PropertyDateTimeFilter<T> : PropertyFilter<T>, IFilter<DateTime>
    {
        public DateTimeFilterType FilterType { get; set; }

        public DateTime Value { get; set; }

        public override Expression<Func<T, bool>> GetExpression()
        {
            var param = Expression.Parameter(typeof(T), "x");
            var member = GetMember(param); // x.PropertyName

            var const1 = Expression.Constant(Value, typeof(DateTime));
            Expression body = FilterType switch
            {
                DateTimeFilterType.Equals => Expression.Equal(member, const1),
                DateTimeFilterType.Before => Expression.LessThan(member, const1),
                DateTimeFilterType.BeforeOrEqual => Expression.LessThanOrEqual(member, const1),
                DateTimeFilterType.After => Expression.GreaterThan(member, const1),
                DateTimeFilterType.AfterOrEqual => Expression.GreaterThanOrEqual(member, const1),
                _ => throw new NotSupportedException()
            };

            return Expression.Lambda<Func<T, bool>>(body, param);
        }
    }

    public enum DateTimeFilterType
    {
        Equals,
        Before,
        BeforeOrEqual,
        After,
        AfterOrEqual
    }
}
