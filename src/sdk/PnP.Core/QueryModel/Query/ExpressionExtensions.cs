using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace System.Linq
{
    internal static class ExpressionExtensions
    {
        public static object GetConstantValue(this Expression expression)
        {
            expression = StripQuotes(expression);
            switch (expression)
            {
                case ConstantExpression ce:
                    return ce.Value;
                case MemberExpression me:
                    object obj = GetConstantValue(me.Expression);
                    return me.Member.GetValue(obj);
            }

            throw new NotSupportedException($"Constant {expression} is invalid. Only {typeof(ConstantExpression)} and {typeof(MemberExpression)} are supported");
        }

        public static Expression StripQuotes(this Expression e)
        {
            while (e is UnaryExpression ue)
            {
                e = ue.Operand;
            }
            return e;
        }
    }
}
