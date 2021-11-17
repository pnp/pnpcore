using PnP.Core;
using System.Linq.Expressions;
using System.Reflection;

namespace System.Linq
{
    internal static class ExpressionExtensions
    {

        internal static Expression<Func<T, object>>[] CastExpressions<T>(this LambdaExpression[] expressions)
        {
            return expressions.Select(e => Expression.Lambda<Func<T, object>>(e.Body, e.Parameters)).ToArray();
        }

        internal static object GetConstantValue(this Expression expression)
        {
            expression = StripQuotes(expression);
            var notSupportedException = new NotSupportedException(string.Format(PnPCoreResources.Exception_Unsupported_ExpressionConstantOnlyTypes, expression, typeof(ConstantExpression), typeof(MemberExpression)));

            switch (expression)
            {
                case ConstantExpression ce:
                    return ce.Value;
                case MemberExpression me:
                    object obj = GetConstantValue(me.Expression);
                    return me.Member.GetValue(obj);
                case NewExpression ne:
                    if (ne.Type.Name == nameof(Guid))
                    {
                        return Expression.Lambda<Func<Guid>>(expression).Compile().Invoke();
                    }

                    throw notSupportedException;
            }

            throw notSupportedException;
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
