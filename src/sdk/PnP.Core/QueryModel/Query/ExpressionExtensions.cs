using PnP.Core;
using System.Linq.Expressions;
using System.Reflection;

namespace System.Linq
{
    internal static class ExpressionExtensions
    {

        internal static Expression<Func<T, object>>[] CastExpressions<T>(this Expression<Func<object, object>>[] expressions)
        {
            return expressions.Select(e => Expression.Lambda<Func<T, object>>(e.Body, e.Parameters)).ToArray();
        }

        internal static object GetConstantValue(this Expression expression)
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

            throw new NotSupportedException(string.Format(PnPCoreResources.Exception_Unsupported_ExpressionConstantOnlyTypes, expression, typeof(ConstantExpression), typeof(MemberExpression)));
        }

        internal static Expression StripQuotes(this Expression e)
        {
            while (e is UnaryExpression ue)
            {
                e = ue.Operand;
            }
            return e;
        }
    }
}
