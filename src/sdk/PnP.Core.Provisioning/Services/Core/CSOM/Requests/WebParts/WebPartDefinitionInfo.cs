using System;
using System.Text.Json;

namespace PnP.Core.Provisioning.Model
{
    /// <summary>
    /// A classic web part instance on a page, as reported by CSOM.
    /// </summary>
    internal sealed class WebPartDefinitionInfo
    {
        /// <summary>
        /// The web part instance id - what a template references to move, update or remove it.
        /// </summary>
        internal Guid Id { get; set; }

        /// <summary>
        /// The web part zone the instance sits in. Empty on a wiki page, where web parts are
        /// positioned by markers embedded in the wiki field rather than by zone.
        /// </summary>
        internal string ZoneId { get; set; }

        /// <summary>
        /// The web part's title, when it was requested.
        /// </summary>
        internal string Title { get; set; }
    }
}

namespace PnP.Core.Provisioning.Services.Core.CSOM.Requests.WebParts
{
    /// <summary>
    /// Reads values out of a web part CSOM response.
    /// </summary>
    internal static class WebPartJson
    {
        /// <summary>
        /// Reads a GUID property, unwrapping CSOM's <c>/Guid(...)/</c> encoding.
        /// </summary>
        internal static Guid ReadGuid(JsonElement element, string propertyName)
        {
            if (!element.TryGetProperty(propertyName, out JsonElement property))
            {
                return Guid.Empty;
            }

            string raw = property.GetString();
            if (string.IsNullOrEmpty(raw))
            {
                return Guid.Empty;
            }

            raw = raw.Replace("/Guid(", "").Replace(")/", "");

            return Guid.TryParse(raw, out Guid value) ? value : Guid.Empty;
        }
    }
}