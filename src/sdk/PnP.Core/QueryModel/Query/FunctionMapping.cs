using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace PnP.Core.QueryModel
{
    /// <summary>
    /// Contains the mapping from methods/members to OData functions
    /// </summary>
    internal static class FunctionMapping
    {
        /// <summary>
        /// List of supported functions
        /// </summary>
        private static readonly FunctionDefinition[] DefinedFunctions =
           {
                Create<string>(s => s.Contains(""), "substringof({1},{0})"),
                Create<string>(s => s.StartsWith(""), "startswith({0},{1})"),
                Create<DateTime>(s => s.Day, "day({0})"),
                Create<DateTime>(s => s.Month, "month({0})"),
                Create<DateTime>(s => s.Year, "year({0})"),
                Create<DateTime>(s => s.Hour, "hour({0})"),
                Create<DateTime>(s => s.Minute, "minute({0})"),
                Create<DateTime>(s => s.Second, "second({0})")
           };

        private static readonly Dictionary<MemberInfo, FunctionDefinition> Mappings = DefinedFunctions.ToDictionary(f => f.Member);

        private static FunctionDefinition Create<T>(Expression<Func<T, object>> expression, string format)
        {
            Expression methodExpression = ((UnaryExpression)expression.Body).Operand;
            // Find the member to map
            MemberInfo member;
            switch (methodExpression)
            {
                case MethodCallExpression mce:
                    member = mce.Method;
                    break;
                case MemberExpression me:
                    member = me.Member;
                    break;
                default:
                    throw new NotSupportedException(string.Format(PnPCoreResources.Exception_Unsupported_ExpressionBody, expression.Body));
            }

            return new FunctionDefinition(member, format);
        }

        /// <summary>
        /// Returns the list of supported methods and properties
        /// </summary>
        public static IEnumerable<MemberInfo> SupportedMembers => Mappings.Keys;

        /// <summary>
        /// Tries to map a member (method/property) and gets the OData formatted function
        /// </summary>
        /// <param name="member">The member to map</param>
        /// <param name="source">The source field name</param>
        /// <param name="arguments">The arguments used for method invocation</param>
        /// <param name="functionCall">The OData function call</param>
        /// <returns></returns>
        public static bool TryMapMember(MemberInfo member, string source, object[] arguments, out string functionCall)
        {
            if (Mappings.TryGetValue(member, out FunctionDefinition functionDefinition))
            {
                functionCall = functionDefinition.Format(source, arguments);
                return true;
            }

            functionCall = null;
            return false;
        }

        private class FunctionDefinition
        {
            private readonly string _format;

            public FunctionDefinition(MemberInfo member, string format)
            {
                _format = format ?? throw new ArgumentNullException(nameof(format));
                Member = member ?? throw new ArgumentNullException(nameof(member));
            }

            public MemberInfo Member { get; }

            public string Format(string source, object[] arguments)
            {
                // Convert all arguments to string
                // Add first the source name
                object[] stringArgs = Enumerable.Repeat(source, 1)
                    .Concat(arguments.Select(ODataUtilities.ConvertToString))
                    .Cast<object>()
                    .ToArray();

                // Format using invariant culture, even if values are already formatted as string
                return string.Format(CultureInfo.InvariantCulture, _format, stringArgs);
            }
        }
    }
}
