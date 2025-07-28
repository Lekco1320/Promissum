using System;
using System.Linq.Expressions;
using System.Reflection;

namespace Lekco.Wpf.Utility.Filter
{
    public class PropertyStringFilter<T> : PropertyFilter<T>, IFilter<string>
    {
        public StringFilterType FilterType { get; set; }

        public string Value { get; set; } = "";

        public override Expression<Func<T, bool>> GetExpression()
        {
            var param = Expression.Parameter(typeof(T), "x");
            var member = GetMember(param); // x.PropertyName
            var constant = Expression.Constant(Value, typeof(string));

            if (FilterType == StringFilterType.Equals)
            {
                var body = Expression.Equal(member, constant);
                return Expression.Lambda<Func<T, bool>>(body, param);
            }

            MethodInfo mi = FilterType switch
            {
                StringFilterType.Contains => typeof(string).GetMethod(nameof(string.Contains), [typeof(string)])!,
                StringFilterType.StartsWith => typeof(string).GetMethod(nameof(string.StartsWith), [typeof(string)])!,
                StringFilterType.EndsWith => typeof(string).GetMethod(nameof(string.EndsWith), [typeof(string)])!,
                _ => null!
            };

            var call = Expression.Call(member, mi, constant);
            return Expression.Lambda<Func<T, bool>>(call, param);
        }
    }

    public enum StringFilterType
    {
        Equals,
        Contains,
        StartsWith,
        EndsWith,
    }
}
