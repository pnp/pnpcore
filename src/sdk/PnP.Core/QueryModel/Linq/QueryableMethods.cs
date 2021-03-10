using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace PnP.Core.QueryModel
{
    /// <summary>
    /// A class that provides reflection metadata for translatable LINQ methods.
    /// </summary>
    internal static class QueryableMethods
    {
        /// <summary>
        /// The <see cref="MethodInfo" /> for <see cref="Queryable.First{TSource}(IQueryable{TSource})" />
        /// </summary>
        public static MethodInfo FirstWithoutPredicate { get; }

        /// <summary>
        /// The <see cref="MethodInfo" /> for <see cref="Queryable.First{TSource}(IQueryable{TSource},Expression{Func{TSource,bool}})" />
        /// </summary>
        public static MethodInfo FirstWithPredicate { get; }

        /// <summary>
        /// The <see cref="MethodInfo" /> for <see cref="Queryable.FirstOrDefault{TSource}(IQueryable{TSource},Expression{Func{TSource,bool}})" />
        /// </summary>
        public static MethodInfo FirstOrDefaultWithoutPredicate { get; }

        /// <summary>
        /// The <see cref="MethodInfo" /> for <see cref="Queryable.FirstOrDefault{TSource}(IQueryable{TSource},Expression{Func{TSource,bool}})" />
        /// </summary>
        public static MethodInfo FirstOrDefaultWithPredicate { get; }

        /// <summary>
        /// The <see cref="MethodInfo" /> for <see cref="QueryableExtensions.QueryProperties{TResult}(IQueryable{TResult}, Expression{Func{TResult, object}}[])" />
        /// </summary>
        public static MethodInfo QueryProperties { get; }

        static QueryableMethods()
        {
            var queryableMethods = typeof(Queryable)
                .GetMethods(BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly).ToList();
            var queryableExtensionsMethods = typeof(QueryableExtensions)
                .GetMethods(BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly).ToList();

            QueryProperties = queryableExtensionsMethods.Single(
                mi => mi.Name == nameof(QueryableExtensions.QueryProperties) && mi.ReturnType.IsGenericType && mi.ReturnType.GetGenericTypeDefinition() == typeof(IQueryable<>));
            FirstWithoutPredicate = queryableMethods.Single(
                mi => mi.Name == nameof(Queryable.First) && mi.GetParameters().Length == 1);
            FirstWithPredicate = queryableMethods.Single(
                mi => mi.Name == nameof(Queryable.First)
                    && mi.GetParameters().Length == 2
                    && IsExpressionOfFunc(mi.GetParameters()[1].ParameterType));
            FirstOrDefaultWithoutPredicate = queryableMethods.Single(
                mi => mi.Name == nameof(Queryable.FirstOrDefault) && mi.GetParameters().Length == 1);
            FirstOrDefaultWithPredicate = queryableMethods.Single(
                mi => mi.Name == nameof(Queryable.FirstOrDefault)
                    && mi.GetParameters().Length == 2
                    && IsExpressionOfFunc(mi.GetParameters()[1].ParameterType));

            static bool IsExpressionOfFunc(Type type, int funcGenericArgs = 2)
                => type.IsGenericType
                    && type.GetGenericTypeDefinition() == typeof(Expression<>)
                    && type.GetGenericArguments()[0].IsGenericType
                    && type.GetGenericArguments()[0].GetGenericArguments().Length == funcGenericArgs;
            
            /* Not used, so commenting it
            static bool IsSelector<T>(Type type)
                => type.IsGenericType
                    && type.GetGenericTypeDefinition() == typeof(Expression<>)
                    && type.GetGenericArguments()[0].IsGenericType
                    && type.GetGenericArguments()[0].GetGenericArguments().Length == 2
                    && type.GetGenericArguments()[0].GetGenericArguments()[1] == typeof(T);
            */
        }
    }
}
