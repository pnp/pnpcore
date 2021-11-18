using System.Linq.Expressions;

namespace PnP.Core.QueryModel
{
    /// <summary>
    /// Translates bool expressions into normal form for eaiser manipulation later on when building REST queries based on LINQ expression. <br/><br/>
    /// <b>Examples:</b> <br/>
    /// <list type="bullet">
    /// <item><para> <code>w.Contains("smth") -> w.Contains("smth") == true</code></para></item>
    /// <item><para> <code>!w.Contains("smth") and w.Title == "smth" and w.HasData -> w.Contains("smth") == false and w.Title == "smth" and w.HasData == true</code></para></item>
    /// </list>
    /// </summary>
    internal class ExpressionNormalizer : ExpressionVisitor
    {
        private Expression initialExpression;

        public ExpressionNormalizer(Expression initial)
        {
            initialExpression = initial;
        }

        /// <summary>
        /// Initial function, which normalizes the expression
        /// </summary>
        /// <returns>New normalized version of the expresion or initial expression, if it's already normalized</returns>
        public Expression Normalize()
        {
            return Visit(initialExpression);
        }

        /// <summary>
        /// Converts l.Contains("smth") to explicit BinaryExpression, ie. <code> l.Contains("smth") == true </code>
        /// </summary>
        /// <param name="body">The initial <see cref="MethodCallExpression"/></param>
        /// <returns>New <see cref="BinaryExpression"/> if conversion is possible</returns>
        protected override Expression VisitMethodCall(MethodCallExpression body)
        {
            if (IsBoolExpression<MethodCallExpression>(body))
            {
                return CreateBinaryBoolExpression(body, true);
            }

            return body;
        }

        /// <summary>
        /// Converts l.HasSmth to explicit BinaryExpression, ie.<code> l.HasSmth == true </code>
        /// </summary>
        /// <param name="body">The initial <see cref="MemberExpression"/></param>
        /// <returns>New <see cref="BinaryExpression"/> if conversion is possible</returns>
        protected override Expression VisitMember(MemberExpression body)
        {
            if (IsBoolExpression<MemberExpression>(body))
            {
                return CreateBinaryBoolExpression(body, true);
            }

            return body;
        }

        /// <summary>
        /// Converts <see cref="UnaryExpression" to the <see cref="BinaryExpression"/> by examining operands./>
        /// </summary>
        /// <param name="unaryExpression"></param>
        /// <returns></returns>
        protected override Expression VisitUnary(UnaryExpression unaryExpression)
        {
            if(IsBoolExpression<MethodCallExpression>(unaryExpression.Operand) || IsBoolExpression<MemberExpression>(unaryExpression.Operand))
            {
                return CreateBinaryBoolExpression(unaryExpression.Operand, false);
            }

            return unaryExpression;
        }

        /// <summary>
        /// If current expression is not normalized, tries to normalize left and right part of the <see cref="BinaryExpression"/>
        /// </summary>
        /// <param name="binaryExpression">Initial <see cref="BinaryExpression"/></param>
        /// <returns>New <see cref="BinaryExpression"/> where left and right parts are normalized</returns>
        protected override Expression VisitBinary(BinaryExpression binaryExpression)
        {
            // if the expression is already normalized, i.e. has a form of l.Contains("smth") == true, then skip it
            if (IsNormalizedBoolExpression<MethodCallExpression>(binaryExpression)
                || IsNormalizedBoolExpression<MemberExpression>(binaryExpression))
            {
                return binaryExpression;
            }

            var left = new ExpressionNormalizer(binaryExpression.Left).Normalize();
            var right = new ExpressionNormalizer(binaryExpression.Right).Normalize();

            return Expression.MakeBinary(binaryExpression.NodeType, left, right);
        }

        /// <summary>
        /// Creates <see cref="BinaryExpression"/> out of a regular expression. <br/><br/>
        /// <b>Examples:</b> <br/>
        /// <list type="bullet">
        /// <item><para> <code>w.Contains("smth") -> w.Contains("smth") == true</code></para></item>
        /// <item><para> <code>w.HasData -> w.HasData == true</code></para></item>
        /// </list>
        /// </summary>
        /// <param name="body"><see cref="Expression"/> instance, wil be used as a left side of the <see cref="BinaryExpression"/></param>
        /// <param name="value"><see cref="bool"/> value, will be used as a constant in the right side of the <see cref="BinaryExpression"/></param>
        /// <returns>New <see cref="BinaryExpression"/></returns>
        private Expression CreateBinaryBoolExpression(Expression body, bool value)
        {
            return Expression.MakeBinary(ExpressionType.Equal, body, Expression.Constant(value));
        }

        /// <summary>
        /// Checks whether <see cref="Expression"/> is of <typeparamref name="T"/> type and whether the return type for the expression is <see cref="bool"/>
        /// </summary>
        /// <typeparam name="T"><see cref="Expression"/> to check against</typeparam>
        /// <param name="body"><see cref="Expression"/> instance to check</param>
        /// <returns><em>true</em> if <paramref name="body"/> is of type <typeparamref name="T"/> and returns <see cref="bool"/></returns>
        private bool IsBoolExpression<T>(Expression body) where T : Expression
        {
            return body is T && body.Type == typeof(bool);
        }

        /// <summary>
        /// Checks whether the left side of the <paramref name="expression"/> is of type <typeparamref name="T"/>, the return type of the right side is <see cref="bool"/><br/><br/>
        /// <b>Examples:</b> <br/>
        /// <list type="bullet">
        /// <item><para> <code>w.Contains("smth") == True -> return true;</code></para></item>
        /// <item><para> <code>w.Contains("smth") -> return false;</code></para></item>
        /// <item><para> <code>w.HasData == False -> return true;</code></para></item>
        /// </list>
        /// </summary>
        /// <typeparam name="T"><see cref="Expression"/> to check against</typeparam>
        /// <param name="expression"><see cref="Expression"/> instance to check</param>
        /// <returns>True, if <paramref name="expression"/> meets conditions</returns>
        private bool IsNormalizedBoolExpression<T>(BinaryExpression expression) where T : Expression
        {
            return expression.Left is T && expression.NodeType == ExpressionType.Equal && expression.Right.Type == typeof(bool);
        }
    }
}
