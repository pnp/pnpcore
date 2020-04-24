using PnP.Core.Model;
using PnP.Core.QueryModel.OData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace PnP.Core.QueryModel.Query
{
    public static class BaseDataModelExtensions
    {
        #region Helper methods to obtain MethodInfo in a safe way

        private static MethodInfo GetMethodInfo<T1, T2>(Func<T1, T2> f, T1 unused1)
        {
            return f.Method;
        }

        private static MethodInfo GetMethodInfo<T1, T2, T3>(Func<T1, T2, T3> f, T1 unused1, T2 unused2)
        {
            return f.Method;
        }

        private static MethodInfo GetMethodInfo<T1, T2, T3, T4>(Func<T1, T2, T3, T4> f, T1 unused1, T2 unused2, T3 unused3)
        {
            return f.Method;
        }

        private static MethodInfo GetMethodInfo<T1, T2, T3, T4, T5>(Func<T1, T2, T3, T4, T5> f, T1 unused1, T2 unused2, T3 unused3, T4 unused4)
        {
            return f.Method;
        }

        private static MethodInfo GetMethodInfo<T1, T2, T3, T4, T5, T6>(Func<T1, T2, T3, T4, T5, T6> f, T1 unused1, T2 unused2, T3 unused3, T4 unused4, T5 unused5)
        {
            return f.Method;
        }

        private static MethodInfo GetMethodInfo<T1, T2, T3, T4, T5, T6, T7>(Func<T1, T2, T3, T4, T5, T6, T7> f, T1 unused1, T2 unused2, T3 unused3, T4 unused4, T5 unused5, T6 unused6)
        {
            return f.Method;
        }

        #endregion

        #region Public extension methods for IQueryable<TResult>

        /// <summary>
        /// Extension method to declare the collection properties to expand while querying the REST service
        /// </summary>
        /// <typeparam name="TResult">The type of the target entity</typeparam>
        /// <param name="collection">The collection of items to expand properties from</param>
        /// <param name="selector">An array of selectors for the expandable properties</param>
        /// <returns>The resulting collection</returns>
        public static IQueryable<TResult> Include<TResult>(
            this IQueryable<TResult> source, Expression<Func<TResult, object>> selector)
        {
            if (source is null)
            {
                throw new ArgumentNullException(nameof(source));
            }
            if (selector is null)
            {
                throw new ArgumentNullException(nameof(selector));
            }

            return source.Provider.CreateQuery<TResult>(
                Expression.Call(
                    null,
                    GetMethodInfo(BaseDataModelExtensions.Include, source, selector),
                    new Expression[] { source.Expression, Expression.Quote(selector) }
                    ));
        }

        /// <summary>
        /// Extension method to declare the collection properties to expand while querying the REST service
        /// </summary>
        /// <typeparam name="TResult">The type of the target entity</typeparam>
        /// <param name="collection">The collection of items to expand properties from</param>
        /// <param name="selector">An array of selectors for the expandable properties</param>
        /// <returns>The resulting collection</returns>
        public static IQueryable<TResult> Include<TResult>(
            this IQueryable<TResult> source, params Expression<Func<TResult, object>>[] selectors)
        {
            if (source is null)
            {
                throw new ArgumentNullException(nameof(source));
            }
            if (selectors is null)
            {
                throw new ArgumentNullException(nameof(selectors));
            }

            IQueryable<TResult> result = source;

            foreach (var s in selectors)
            {
                result = result.Include(s);
            }

            return result;
        }

        /// <summary>
        /// Extension method to declare a field/metadata property to load while executing the REST query
        /// </summary>
        /// <typeparam name="TResult">The type of the target entity</typeparam>
        /// <param name="collection">The collection of items to load the field/metadata from</param>
        /// <param name="selector">A selector for a field/metadata</param>
        /// <returns>The resulting collection</returns>
        public static IQueryable<TResult> Load<TResult>(
            this IQueryable<TResult> source, Expression<Func<TResult, object>> selector)
        {
            if (source is null)
            {
                throw new ArgumentNullException(nameof(source));
            }
            if (selector is null)
            {
                throw new ArgumentNullException(nameof(selector));
            }

            return source.Provider.CreateQuery<TResult>(
                Expression.Call(
                    null,
                    GetMethodInfo(BaseDataModelExtensions.Load, source, selector),
                    new Expression[] { source.Expression, Expression.Quote(selector) }
                    ));
        }

        /// <summary>
        /// Extension method to declare the fields/metadata properties to load while executing the REST query
        /// </summary>
        /// <typeparam name="TResult">The type of the target entity</typeparam>
        /// <param name="collection">The collection of items to load fields/metadata from</param>
        /// <param name="selectors">An array of selectors for the fields/metadata</param>
        /// <returns>The resulting collection</returns>
        public static IQueryable<TResult> Load<TResult>(
            this IQueryable<TResult> source, params Expression<Func<TResult, object>>[] selectors)
        {
            if (source is null)
            {
                throw new ArgumentNullException(nameof(source));
            }
            if (selectors is null)
            {
                throw new ArgumentNullException(nameof(selectors));
            }

            IQueryable<TResult> result = source;

            foreach (var s in selectors)
            {
                result = result.Load(s);
            }

            return result;
        }

        #endregion
    }
}
