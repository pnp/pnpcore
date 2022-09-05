using Microsoft.SharePoint.Client;
using System.Collections.Generic;
using System.Linq;

namespace PnP.Core.Transformation.SharePoint.Extensions
{
    /// <summary>
    /// This class provides extension methods that will help you work with fields and content types.
    /// </summary>
    internal static partial class FieldAndContentTypeExtensions
    {
        /// <summary>
        /// Returns the field if it exists. Null if does not exist.
        /// </summary>
        /// <param name="web">Web to be processed</param>
        /// <param name="internalName">If true, search parent sites and root site</param>
        /// <param name="searchInSiteHierarchy">If true, search across all the available fields in the site hierarchy</param>
        /// <returns>Field</returns>
        public static Field GetFieldByInternalName(this Web web, string internalName, bool searchInSiteHierarchy = false)
        {
            IEnumerable<Field> fields = null;

            if (searchInSiteHierarchy)
            {
                fields = web.Context.LoadQuery(web.AvailableFields.Where(f => f.InternalName == internalName));
            }
            else
            {
                fields = web.Context.LoadQuery(web.Fields.Where(f => f.InternalName == internalName));
            }

            web.Context.ExecuteQueryRetry();
            return fields.FirstOrDefault();
        }

        /// <summary>
        /// Returns the field if it exists. Null if it does not exist.
        /// </summary>
        /// <param name="fields">FieldCollection to be processed.</param>
        /// <param name="internalName">Internal name of the field</param>
        /// <returns>Field</returns>
        public static Field GetFieldByInternalName(this FieldCollection fields, string internalName)
        {
            if (!fields.ServerObjectIsNull.HasValue ||
                fields.ServerObjectIsNull.Value)
            {
                fields.Context.Load(fields);
                fields.Context.ExecuteQueryRetry();
            }
            return fields.FirstOrDefault(f => f.InternalName == internalName);
        }
    }
}
