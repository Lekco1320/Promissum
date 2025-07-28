using System;
using System.Linq.Expressions;

namespace Lekco.Wpf.Utility.Filter
{
    public class PropertyFileSizeFilter<T> : PropertyFilter<T>, IFilter<string>
    {
        public FileSizeFilterType FilterType { get; set; }

        public FileSize Value { get; set; }

        public override Expression<Func<T, bool>> GetExpression()
        {
            var param = Expression.Parameter(typeof(T), "x");
            var member = GetMember(param); // x.PropertyName

            var const1 = member.Type == typeof(FileSize) ?
                Expression.Constant(Value, typeof(FileSize)) :
                Expression.Constant(Value.SizeInBytes, typeof(long));
            Expression body = FilterType switch
            {
                FileSizeFilterType.Equals => Expression.Equal(member, const1),
                FileSizeFilterType.LessThan => Expression.LessThan(member, const1),
                FileSizeFilterType.LessThanOrEqual => Expression.LessThanOrEqual(member, const1),
                FileSizeFilterType.GreaterThan => Expression.GreaterThan(member, const1),
                FileSizeFilterType.GreaterThanOrEqual => Expression.GreaterThanOrEqual(member, const1),
                _ => throw new NotSupportedException()
            };

            return Expression.Lambda<Func<T, bool>>(body, param);
        }
    }

    public enum FileSizeFilterType
    {
        Equals,
        LessThan,
        LessThanOrEqual,
        GreaterThan,
        GreaterThanOrEqual,
    }
}
