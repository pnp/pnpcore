using System;
using System.Text.Json;

namespace PnP.Core.Provisioning.Model
{
    /// <summary>
    /// A term group, as read back from the taxonomy CSOM API.
    /// </summary>
    internal sealed class TermGroupInfo
    {
        internal Guid Id { get; set; }

        internal string Name { get; set; }

        internal string Description { get; set; }

        internal bool IsSiteCollectionGroup { get; set; }
    }

    /// <summary>
    /// A term set, as read back from the taxonomy CSOM API.
    /// </summary>
    internal sealed class TermSetInfo
    {
        internal Guid Id { get; set; }

        internal string Name { get; set; }

        internal string Description { get; set; }
    }

    /// <summary>
    /// A term, as read back from the taxonomy CSOM API.
    /// </summary>
    internal sealed class TermInfo
    {
        internal Guid Id { get; set; }

        internal string Name { get; set; }
    }
}

namespace PnP.Core.Provisioning.Services.Core.CSOM.Requests.Taxonomy
{
    /// <summary>
    /// Reads values out of a CSOM taxonomy JSON response.
    /// </summary>
    internal static class TaxonomyJson
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
