using System;
using System.Linq.Expressions;
using System.Numerics;

namespace Lekco.Wpf.Utility.Filter
{
    public class PropertyNumericFilter<T, TNumber> : PropertyFilter<T>, IFilter<TNumber>
        where TNumber : struct, INumber<TNumber>
    {
        public NumericFilterType FilterType { get; set; }

        public TNumber Value { get; set; }

        public override Expression<Func<T, bool>> GetExpression()
        {
            var param  = Expression.Parameter(typeof(T), "x");
            var member = GetMember(param); // x.PropertyName
            var constant1 = Expression.Constant(Convert.ChangeType(Value, typeof(TNumber)), typeof(TNumber));

            Expression body = FilterType switch
            {
                NumericFilterType.Equals => Expression.Equal(member, constant1),
                NumericFilterType.GreaterThan => Expression.GreaterThan(member, constant1),
                NumericFilterType.GreaterThanOrEqual => Expression.GreaterThanOrEqual(member, constant1),
                NumericFilterType.LessThan => Expression.LessThan(member, constant1),
                NumericFilterType.LessThanOrEqual => Expression.LessThanOrEqual(member, constant1),
                _ => throw new NotSupportedException()
            };

            return Expression.Lambda<Func<T, bool>>(body, param);
        }
    }

    public enum NumericFilterType
    {
        Equals,
        LessThan,
        LessThanOrEqual,
        GreaterThan,
        GreaterThanOrEqual,
    }

    public class PropertyIntFilter<T> : PropertyNumericFilter<T, int>
    {
    }

    public class PropertyLongFilter<T> : PropertyNumericFilter<T, long>
    {
    }

    public class PropertyDecimalFilter<T> : PropertyNumericFilter<T, decimal>
    {
    }

    public class PropertyFloatFilter<T> : PropertyNumericFilter<T, float>
    {
    }

    public class PropertyDoubleFilter<T> : PropertyNumericFilter<T, double>
    {
    }
}
