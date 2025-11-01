using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Lekco.Wpf.Utility.Sorter
{
    public abstract class PropertySorter
    {
        public string? PropertyName { get; set; }

        public bool Descending { get; set; } = false;
    }

    public class PropertySorter<T> : PropertySorter
    {
        public PropertySorter(string? propertyName, bool descending)
        {
            PropertyName = propertyName;
            Descending = descending;
        }

        public PropertySorter()
            : this(null, false)
        {
        }

        public virtual Expression AddQueryExpression(Expression exp, bool first)
        {
            if (string.IsNullOrWhiteSpace(PropertyName))
            {
                throw new ArgumentException("PropertyName cannot be null or empty.", nameof(PropertyName));
            }

            var accessInfo = BuildMemberAccess(PropertyName);
            var keySelector = Expression.Lambda(accessInfo.Member, accessInfo.Parameter);
            string methodName = first
                ? (Descending ? nameof(Queryable.OrderByDescending) : nameof(Queryable.OrderBy))
                : (Descending ? nameof(Queryable.ThenByDescending) : nameof(Queryable.ThenBy));

            var call = Expression.Call(
                typeof(Queryable),
                methodName,
                new Type[] { typeof(T), accessInfo.MemberType },
                exp,
                Expression.Quote(keySelector)
            );

            return call;
        }

        protected virtual MemberAccessInfo BuildMemberAccess(string path)
        {
            var param = Expression.Parameter(typeof(T), "x");
            Expression cur = param;
            Type curType = typeof(T);

            foreach (var name in path.Split('.'))
            {
                var prop = curType.GetProperty(name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.IgnoreCase);
                if (prop != null)
                {
                    cur = Expression.Property(cur, prop);
                    curType = prop.PropertyType;
                    continue;
                }

                var field = curType.GetField(name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.IgnoreCase);
                if (field != null)
                {
                    cur = Expression.Field(cur, field);
                    curType = field.FieldType;
                    continue;
                }

                throw new ArgumentException($"Cannot find property or field '{name}' on type '{curType.Name}'.");
            }

            return new MemberAccessInfo(param, cur, curType);
        }
    }
}
