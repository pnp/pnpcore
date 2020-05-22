using PnP.Core.Model;
using PnP.Core.QueryModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace PnP.Core.QueryModel
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
                    GetMethodInfo(Include, source, selector),
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
                    GetMethodInfo(Load, source, selector),
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
        public static Core.Model.SharePoint.IList GetByTitle(
            this IQueryable<Core.Model.SharePoint.IList> source, string title)
        {
            // Just rely on the below overload, without providing any selector
            return source.GetByTitle(title, null);
        }

        /// <summary>
        /// Extension method to select a list (IList) by title
        /// </summary>
        /// <param name="source">The collection of lists to get the list by title from</param>
        /// <param name="title">The title to search for</param>
        /// <param name="selectors">The expressions declaring the fields to select</param>
        /// <returns>The resulting list instance, if any</returns>
        public static Core.Model.SharePoint.IList GetByTitle(
            this IQueryable<Core.Model.SharePoint.IList> source,
            string title,
            params Expression<Func<Core.Model.SharePoint.IList, object>>[] selectors)
        {
            if (source is null)
            {
                throw new ArgumentNullException(nameof(source));
            }
            if (title is null)
            {
                throw new ArgumentNullException(nameof(title));
            }

            IQueryable<Core.Model.SharePoint.IList> selectionTarget = source;

            if (selectors != null)
            {
                foreach (var s in selectors)
                {
                    selectionTarget = selectionTarget.Load(s);
                }
            }

            Expression<Func<Core.Model.SharePoint.IList, bool>> predicate = l => l.Title == title;

            return source.Provider.Execute<Core.Model.SharePoint.IList>(
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
        public static Core.Model.SharePoint.IListItem GetById(
            this IQueryable<Core.Model.SharePoint.IListItem> source, int id)
        {
            // Just rely on the below overload, without providing any selector
            return source.GetById(id, null);
        }

        /// <summary>
        /// Extension method to select a list item (IListItem) by Id
        /// </summary>
        /// <param name="source">The collection of lists items to get the item by Id from</param>
        /// <param name="id">The Id to search for</param>
        /// <param name="selectors">The expressions declaring the fields to select</param>
        /// <returns>The resulting list item instance, if any</returns>
        public static Core.Model.SharePoint.IListItem GetById(
            this IQueryable<Core.Model.SharePoint.IListItem> source,
            int id,
            params Expression<Func<Core.Model.SharePoint.IListItem, object>>[] selectors)
        {
            if (source is null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            IQueryable<Core.Model.SharePoint.IListItem> selectionTarget = source;

            if (selectors != null)
            {
                foreach (var s in selectors)
                {
                    selectionTarget = selectionTarget.Load(s);
                }
            }

            Expression<Func<Core.Model.SharePoint.IListItem, bool>> predicate = l => l.Id == id;

            return source.Provider.Execute<Core.Model.SharePoint.IListItem>(
                Expression.Call(
                    null,
                    GetMethodInfo(Queryable.FirstOrDefault, selectionTarget, predicate),
                    new Expression[] { selectionTarget.Expression, Expression.Quote(predicate) }
                    ));
        }

        #endregion

        #region GetByDisplayName for Channels implementation

        /// <summary>
        /// Extension method to select a channel (ITeamChannel) by displayName
        /// </summary>
        /// <param name="source">The collection of channels to get the channel by displayName from</param>
        /// <param name="displayName">The displayName to search for</param>
        /// <returns>The resulting channel instance, if any</returns>
        public static Core.Model.Teams.ITeamChannel GetByDisplayName(
            this IQueryable<Core.Model.Teams.ITeamChannel> source, string displayName)
        {
            // Just rely on the below overload, without providing any selector
            return source.GetByDisplayName(displayName, null);
        }

        /// <summary>
        /// Extension method to select a channel (ITeamChannel) by displayName
        /// </summary>
        /// <param name="source">The collection of channels to get the channel by displayName from</param>
        /// <param name="displayName">The displayName to search for</param>
        /// <param name="selectors">The expressions declaring the fields to select</param>
        /// <returns>The resulting channel instance, if any</returns>
        public static Core.Model.Teams.ITeamChannel GetByDisplayName(
            this IQueryable<Core.Model.Teams.ITeamChannel> source,
            string displayName,
            params Expression<Func<Core.Model.Teams.ITeamChannel, object>>[] selectors)
        {
            if (source is null)
            {
                throw new ArgumentNullException(nameof(source));
            }
            if (displayName is null)
            {
                throw new ArgumentNullException(nameof(displayName));
            }

            IQueryable<Core.Model.Teams.ITeamChannel> selectionTarget = source;

            if (selectors != null)
            {
                foreach (var s in selectors)
                {
                    selectionTarget = selectionTarget.Load(s);
                }
            }

            Expression<Func<Core.Model.Teams.ITeamChannel, bool>> predicate = c => c.DisplayName == displayName;

            return source.Provider.CreateQuery<Core.Model.Teams.ITeamChannel>(
                Expression.Call(
                    null,
                    GetMethodInfo(Queryable.Where, selectionTarget, predicate),
                    new Expression[] { selectionTarget.Expression, Expression.Quote(predicate) }
                    )).ToList().FirstOrDefault();
        }

        #endregion

        #endregion
    }
}
