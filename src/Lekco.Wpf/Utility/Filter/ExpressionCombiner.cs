using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace Lekco.Wpf.Utility.Filter
{
    public static class ExpressionCombiner
    {
        /// <summary>
        /// 把多个 expr 用 AND 合并：expr1(x) && expr2(x) && ...
        /// </summary>
        public static Expression<Func<T, bool>> AndAlsoAll<T>(
            IEnumerable<Expression<Func<T, bool>>> expressions)
        {
            return CombineAll(expressions, Expression.AndAlso);
        }

        /// <summary>
        /// 把多个 expr 用 OR 合并：expr1(x) || expr2(x) || ...
        /// </summary>
        public static Expression<Func<T, bool>> OrElseAll<T>(
            IEnumerable<Expression<Func<T, bool>>> expressions)
        {
            return CombineAll(expressions, Expression.OrElse);
        }

        public static Expression<Func<T, bool>> CombineAll<T>(
            IEnumerable<Expression<Func<T, bool>>> expressions,
            Func<Expression, Expression, BinaryExpression> mergeFunc
        )
        {
            var exprList = expressions.Where(e => e != null).ToList();
            if (exprList.Count == 0)
            {
                return x => true;     // 没条件，返回恒真
            }
            if (exprList.Count == 1)
            {
                return exprList[0];
            }

            // 用同一个参数 x
            var param = Expression.Parameter(typeof(T), "x");

            // 把每个 expr 的 Body 中原先的 param 替换成新 param
            Expression? body = null;
            foreach (var exp in exprList)
            {
                var replaced = ParameterReplacer.Replace(exp.Body, exp.Parameters[0], param);

                body = body == null
                    ? replaced
                    : mergeFunc(body, replaced);
            }

            return Expression.Lambda<Func<T, bool>>(body!, param);
        }

        /// <summary>
        /// 用来把 expression 树里 oldParam 全部替换成 newParam
        /// </summary>
        class ParameterReplacer : ExpressionVisitor
        {
            private readonly ParameterExpression _oldParam;
            private readonly ParameterExpression _newParam;

            public ParameterReplacer(ParameterExpression oldParam, ParameterExpression newParam)
            {
                _oldParam = oldParam;
                _newParam = newParam;
            }

            protected override Expression VisitParameter(ParameterExpression node)
                => node == _oldParam ? _newParam : base.VisitParameter(node);

            public static Expression Replace(
                Expression body,
                ParameterExpression oldParam,
                ParameterExpression newParam)
            {
                return new ParameterReplacer(oldParam, newParam).Visit(body);
            }
        }
    }
}
