using System;
using System.Linq.Expressions;

namespace Lekco.Wpf.Utility.Sorter
{
    public class MemberAccessInfo
    {
        public ParameterExpression Parameter { get; set; }

        public Expression Member { get; set; }

        public Type MemberType { get; set; }

        public MemberAccessInfo(ParameterExpression parameter, Expression member, Type memberType)
        {
            Parameter = parameter;
            Member = member;
            MemberType = memberType;
        }
    }
}
