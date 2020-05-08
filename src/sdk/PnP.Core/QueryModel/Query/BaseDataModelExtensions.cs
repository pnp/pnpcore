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

        #region Include implementation

        /// <summary>
        /// Extension method to declare the collection properties to expand while querying the REST service
        /// </summary>
        /// <typeparam name="TResult">The type of the target entity</typeparam>
        /// <param name="source">The collection of items to expand properties from</param>
        /// <param name="selector">A selector for the expandable properties</param>
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
        /// <param name="source">The collection of items to expand properties from</param>
        /// <param name="selectors">An array of selectors for the expandable properties</param>
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

        #endregion

        #region Load implementation

        /// <summary>
        /// Extension method to declare a field/metadata property to load while executing the REST query
        /// </summary>
        /// <typeparam name="TResult">The type of the target entity</typeparam>
        /// <param name="source">The collection of items to load the field/metadata from</param>
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
        /// <param name="source">The collection of items to load fields/metadata from</param>
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

        #region GetByTitle for Lists implementation

        /// <summary>
        /// Extension method to select a list (IList) by title
        /// </summary>
        /// <param name="source">The collection of lists to get the list by title from</param>
        /// <param name="title">The title to search for</param>
        /// <returns>The resulting list instance, if any</returns>
        public static PnP.Core.Model.SharePoint.IList GetByTitle(
            this IQueryable<PnP.Core.Model.SharePoint.IList> source, string title)
        {
            // Just rely on the below overload, without providing any selector
            return GetByTitle(source, title, null);
        }

        /// <summary>
        /// Extension method to select a list (IList) by title
        /// </summary>
        /// <param name="source">The collection of lists to get the list by title from</param>
        /// <param name="title">The title to search for</param>
        /// <param name="selectors">The expressions declaring the fields to select</param>
        /// <returns>The resulting list instance, if any</returns>
        public static PnP.Core.Model.SharePoint.IList GetByTitle(
            this IQueryable<PnP.Core.Model.SharePoint.IList> source, 
            string title, 
            params Expression<Func<PnP.Core.Model.SharePoint.IList, object>>[] selectors)
        {
            if (source is null)
            {
                throw new ArgumentNullException(nameof(source));
            }
            if (title is null)
            {
                throw new ArgumentNullException(nameof(title));
            }

            IQueryable<PnP.Core.Model.SharePoint.IList> selectionTarget = source;

            if (selectors != null)
            {
                foreach (var s in selectors)
                {
                    selectionTarget = selectionTarget.Load(s);
                }
            }

            Expression<Func<PnP.Core.Model.SharePoint.IList, bool>> predicate = l => l.Title == title;

            return source.Provider.Execute<PnP.Core.Model.SharePoint.IList>(
                Expression.Call(
                    null,
                    GetMethodInfo(Queryable.FirstOrDefault, selectionTarget, predicate),
                    new Expression[] { selectionTarget.Expression, Expression.Quote(predicate) }
                    ));
        }

        #endregion

        #region GetById for List Items implementation

        /// <summary>
        /// Extension method to select a list item (IListItem) by Id
        /// </summary>
        /// <param name="source">The collection of lists items to get the item by Id from</param>
        /// <param name="id">The Id to search for</param>
        /// <returns>The resulting list item instance, if any</returns>
        public static PnP.Core.Model.SharePoint.IListItem GetById(
            this IQueryable<PnP.Core.Model.SharePoint.IListItem> source, int id)
        {
            // Just rely on the below overload, without providing any selector
            return GetById(source, id, null);
        }

        /// <summary>
        /// Extension method to select a list item (IListItem) by Id
        /// </summary>
        /// <param name="source">The collection of lists items to get the item by Id from</param>
        /// <param name="id">The Id to search for</param>
        /// <param name="selectors">The expressions declaring the fields to select</param>
        /// <returns>The resulting list item instance, if any</returns>
        public static PnP.Core.Model.SharePoint.IListItem GetById(
            this IQueryable<PnP.Core.Model.SharePoint.IListItem> source,
            int id,
            params Expression<Func<PnP.Core.Model.SharePoint.IListItem, object>>[] selectors)
        {
            if (source is null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            IQueryable<PnP.Core.Model.SharePoint.IListItem> selectionTarget = source;

            if (selectors != null)
            {
                foreach (var s in selectors)
                {
                    selectionTarget = selectionTarget.Load(s);
                }
            }

            Expression<Func<PnP.Core.Model.SharePoint.IListItem, bool>> predicate = l => l.Id == id;

            return source.Provider.Execute<PnP.Core.Model.SharePoint.IListItem>(
                Expression.Call(
                    null,
                    GetMethodInfo(Queryable.FirstOrDefault, selectionTarget, predicate),
                    new Expression[] { selectionTarget.Expression, Expression.Quote(predicate) }
                    ));
        }

        #endregion

        #endregion
    }
}
