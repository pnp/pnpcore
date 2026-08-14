using Microsoft.Extensions.Logging;
using PnP.Core.Model.SharePoint;
using PnP.Core.Services;
using System;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace PnP.Core.Provisioning.ObjectHandlers.Utilities
{
    /// <summary>
    /// Fixes up a field's schema XML before it is sent to SharePoint.
    /// </summary>
    internal static class FieldUtilities
    {
        /// <summary>
        /// Turns a lookup field's list <em>url</em> into the list's id.
        /// </summary>
        /// <param name="context">The site being provisioned</param>
        /// <param name="fieldXml">The field's schema XML</param>
        /// <param name="reportUnresolved">
        /// Called with the unresolved target when the list cannot be found. <b>SharePoint accepts a
        /// lookup whose <c>List</c> attribute is a url it cannot resolve</b> and creates a column
        /// that points at nothing - no error, and the column looks fine until someone uses it. The
        /// caller reports it, because silence here is indistinguishable from success.
        /// </param>
        internal static async Task<string> FixLookupFieldAsync(PnPContext context, string fieldXml, Action<string> reportUnresolved = null)
        {
            XElement fieldElement = XElement.Parse(fieldXml);
            string fieldType = (string)fieldElement.Attribute("Type");

            if (fieldType != "Lookup" && fieldType != "LookupMulti")
            {
                return fieldXml;
            }

            string listAttribute = (string)fieldElement.Attribute("List");

            if (string.IsNullOrEmpty(listAttribute) || Guid.TryParse(listAttribute, out _))
            {
                return fieldXml;
            }

            try
            {
                string serverRelativeUrl = $"{context.Web.ServerRelativeUrl.TrimEnd('/')}/{listAttribute.TrimStart('/')}";
                IList targetList = await context.Web.Lists.GetByServerRelativeUrlAsync(serverRelativeUrl, l => l.Id).ConfigureAwait(false);

                if (targetList != null)
                {
                    fieldElement.SetAttributeValue("List", targetList.Id.ToString("B"));
                    return fieldElement.ToString();
                }
            }
            catch (Exception ex)
            {
                context.Logger?.LogDebug(ex, "{Source}: could not resolve the lookup target '{List}'.",
                    Constants.LOGGING_SOURCE, listAttribute);
            }

            reportUnresolved?.Invoke(listAttribute);

            return fieldXml;
        }
    }
}
