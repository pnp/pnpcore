using Microsoft.SharePoint.Client;
using PnP.Core.Transformation.SharePoint.Model;
using System;
using System.Collections.Generic;
using System.Text;

namespace PnP.Core.Transformation.SharePoint.Extensions
{
    /// <summary>
    /// Extension class for ListItem objects
    /// </summary>
    internal static class ListItemExtensions
    {
        /// <summary>
        /// Determines the type of page
        /// </summary>
        /// <param name="item">Page list item</param>
        /// <returns>Type of page</returns>
        internal static SourcePageType PageType(this ListItem item)
        {
            if (FieldExistsAndUsed(item, SharePointConstants.HtmlFileTypeField) && !String.IsNullOrEmpty(item[SharePointConstants.HtmlFileTypeField].ToString()))
            {
                if (item[SharePointConstants.HtmlFileTypeField].ToString().Equals("SharePoint.WebPartPage.Document", StringComparison.InvariantCultureIgnoreCase))
                {
                    return SourcePageType.WebPartPage;
                }
            }

            if (FieldExistsAndUsed(item, SharePointConstants.WikiField) && !String.IsNullOrEmpty(item[SharePointConstants.WikiField].ToString()))
            {
                return SourcePageType.WikiPage;
            }

            if (FieldExistsAndUsed(item, SharePointConstants.BodyField) && !String.IsNullOrEmpty(item[SharePointConstants.BodyField].ToString()))
            {
                return SourcePageType.BlogPage;
            }

            if (FieldExistsAndUsed(item, SharePointConstants.ClientSideApplicationIdField) && item[SharePointConstants.ClientSideApplicationIdField].ToString().Equals(SharePointConstants.FeatureId_Web_ModernPage.ToString(), StringComparison.InvariantCultureIgnoreCase))
            {
                return SourcePageType.ClientSidePage;
            }

            if (FieldExists(item, SharePointConstants.PublishingRollupImageField) && FieldExists(item, SharePointConstants.AudienceField))
            {
                return SourcePageType.PublishingPage;
            }

            if (FieldExistsAndUsed(item, SharePointConstants.WikiField))
            {
                return SourcePageType.WikiPage;
            }

            if (FieldExistsAndUsed(item, SharePointConstants.FileTypeField) && !String.IsNullOrEmpty(item[SharePointConstants.FileTypeField].ToString()))
            {
                if (item[SharePointConstants.FileTypeField].ToString().Equals("pointpub", StringComparison.InvariantCultureIgnoreCase))
                {
                    return SourcePageType.DelveBlogPage;
                }
            }

            return SourcePageType.AspxPage;
        }

        /// <summary>
        /// Checks if a listitem contains a field
        /// </summary>
        /// <param name="item">List item to check</param>
        /// <param name="fieldName">Name of the field to check</param>
        /// <returns></returns>
        internal static bool FieldExists(this ListItem item, string fieldName)
        {
            return item.FieldValues.ContainsKey(fieldName);
        }

        /// <summary>
        /// Checks if a listitem contains a field with a value
        /// </summary>
        /// <param name="item">List item to check</param>
        /// <param name="fieldName">Name of the field to check</param>
        /// <returns></returns>
        internal static bool FieldExistsAndUsed(this ListItem item, string fieldName)
        {
            return (item.FieldValues.ContainsKey(fieldName) && item[fieldName] != null);
        }

        /// <summary>
        /// Checks if a listitem contains a field and eventually retrieves its value
        /// </summary>
        /// <param name="item">List item to check</param>
        /// <param name="fieldName">Name of the field to check</param>
        /// <param name="fieldValue">The value of the field, if any</param>
        /// <returns>Whether the field is defined in the target item</returns>
        internal static bool TryGetFieldValue(this ListItem item, string fieldName, out string fieldValue)
        {
            if (item.FieldValues.ContainsKey(fieldName) && item[fieldName] != null)
            {
                fieldValue = item[fieldName].ToString();
                return true;
            }
            else
            {
                fieldValue = null;
                return false;
            }
        }
    }
}
