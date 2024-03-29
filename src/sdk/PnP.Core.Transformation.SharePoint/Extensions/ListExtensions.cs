﻿using Microsoft.SharePoint.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace PnP.Core.Transformation.SharePoint.Extensions
{
    internal static class ListExtensions
    {
        /// <summary>
        /// Get list by using Title
        /// </summary>
        /// <param name="web">Site to be processed - can be root web or sub site</param>
        /// <param name="listTitle">Title of the list to return</param>
        /// <returns>Loaded list instance matching to title or null</returns>
        /// <exception cref="System.ArgumentException">Thrown when listTitle is a zero-length string or contains only white space</exception>
        /// <exception cref="System.ArgumentNullException">listTitle is null</exception>
        /// <param name="expressions">Additional list of lambda expressions of properties to load alike l => l.BaseType</param>
        public static List GetListByTitle(this Web web, string listTitle, params Expression<Func<List, object>>[] expressions)
        {
            var baseExpressions = new List<Expression<Func<List, object>>> { l => l.DefaultViewUrl, l => l.Id, l => l.BaseTemplate, l => l.OnQuickLaunch, l => l.DefaultViewUrl, l => l.Title, l => l.Hidden, l => l.RootFolder };

            if (expressions != null && expressions.Any())
            {
                baseExpressions.AddRange(expressions);
            }
            if (string.IsNullOrEmpty(listTitle))
            {
                throw (listTitle == null)
                    ? new ArgumentNullException(nameof(listTitle))
                    : new ArgumentException(SharePointTransformationResources.Error_Message_EmptyString_Arg, nameof(listTitle));
            }
            var query = web.Lists.IncludeWithDefaultProperties(baseExpressions.ToArray());
            var lists = web.Context.LoadQuery(query.Where(l => l.Title == listTitle));
            web.Context.ExecuteQueryRetry();
            return lists.FirstOrDefault();
        }

        /// <summary>
        /// Get List by using Id
        /// </summary>
        /// <param name="web">The web containing the list</param>
        /// <param name="listId">The Id of the list</param>
        /// <param name="expressions">Additional list of lambda expressions of properties to load alike l => l.BaseType</param>
        /// <returns>Loaded list instance matching specified Id</returns>
        /// <exception cref="System.ArgumentException">Thrown when listId is an empty Guid</exception>
        /// <exception cref="System.ArgumentNullException">listId is null</exception>
        public static List GetListById(this Web web, Guid listId, params Expression<Func<List, object>>[] expressions)
        {
            var baseExpressions = new List<Expression<Func<List, object>>> { l => l.DefaultViewUrl, l => l.Id, l => l.BaseTemplate, l => l.OnQuickLaunch, l => l.DefaultViewUrl, l => l.Title, l => l.Hidden, l => l.RootFolder };

            if (expressions != null && expressions.Any())
            {
                baseExpressions.AddRange(expressions);
            }

            if (listId == Guid.Empty)
            {
                throw new ArgumentNullException(nameof(listId));
            }

            if (listId == Guid.Empty)
            {
                throw new ArgumentException(nameof(listId));
            }

            var query = web.Lists.IncludeWithDefaultProperties(baseExpressions.ToArray());
            var lists = web.Context.LoadQuery(query.Where(l => l.Id == listId));

            web.Context.ExecuteQueryRetry();

            return lists.FirstOrDefault();
        }

        /// <summary>
        /// Get list by using Url
        /// </summary>
        /// <param name="web">Web (site) to be processed</param>
        /// <param name="webRelativeUrl">Url of list relative to the web (site), e.g. lists/testlist</param>
        /// <param name="expressions">Additional list of lambda expressions of properties to load alike l => l.BaseType</param>
        /// <returns>Returns list if found, null if no list is found.</returns>
        public static List GetListByUrl(this Web web, string webRelativeUrl, params Expression<Func<List, object>>[] expressions)
        {
            var baseExpressions = new List<Expression<Func<List, object>>> { l => l.DefaultViewUrl, l => l.Id, l => l.BaseTemplate, l => l.OnQuickLaunch, l => l.DefaultViewUrl, l => l.Title, l => l.Hidden, l => l.RootFolder };

            if (expressions != null && expressions.Any())
            {
                baseExpressions.AddRange(expressions);
            }
            if (string.IsNullOrEmpty(webRelativeUrl))
                throw new ArgumentNullException(nameof(webRelativeUrl));

            if (!web.IsObjectPropertyInstantiated("ServerRelativeUrl"))
            {
                web.Context.Load(web, w => w.ServerRelativeUrl);
                web.Context.ExecuteQueryRetry();
            }
            var listServerRelativeUrl = UrlUtility.Combine(web.ServerRelativeUrl, webRelativeUrl);

            var foundList = web.GetList(listServerRelativeUrl.ToString());
            web.Context.Load(foundList, baseExpressions.ToArray());
            try
            {
                web.Context.ExecuteQueryRetry();
            }
            catch (ServerException se)
            {
                if (se.ServerErrorTypeName == "System.IO.FileNotFoundException")
                {
                    foundList = null;
                }
                else
                {
                    throw;
                }
            }

            return foundList;
        }
    }
}
