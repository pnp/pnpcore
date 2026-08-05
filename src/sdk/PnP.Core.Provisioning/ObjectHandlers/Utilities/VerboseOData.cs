using System;
using System.Collections.Generic;
using System.Text.Json;

namespace PnP.Core.Provisioning.ObjectHandlers.Utilities
{
    /// <summary>
    /// Reads the verbose OData shapes SharePoint's REST endpoints answer with.
    /// </summary>
    internal static class VerboseOData
    {
        /// <summary>
        /// Strips the <c>d</c> / method-name / <c>results</c> envelope off a response.
        /// </summary>
        internal static JsonElement Unwrap(JsonElement root)
        {
            JsonElement current = root;

            if (current.ValueKind == JsonValueKind.Object
                && current.TryGetProperty("d", out JsonElement wrapper))
            {
                current = wrapper;

                if (SingleMember(current, out JsonProperty only)
                    && !string.Equals(only.Name, "__metadata", StringComparison.Ordinal))
                {
                    current = only.Value;
                }
            }

            if (current.ValueKind == JsonValueKind.Object
                && current.TryGetProperty("results", out JsonElement results))
            {
                current = results;
            }

            return current;
        }

        private static bool SingleMember(JsonElement element, out JsonProperty only)
        {
            only = default;

            if (element.ValueKind != JsonValueKind.Object)
            {
                return false;
            }

            int count = 0;

            foreach (JsonProperty property in element.EnumerateObject())
            {
                if (++count > 1)
                {
                    return false;
                }

                only = property;
            }

            return count == 1;
        }

        /// <summary>
        /// Reads a collection property, which verbose OData writes as <c>{"results":[…]}</c>.
        /// </summary>
        internal static IEnumerable<JsonElement> CollectionOf(JsonElement element, string name)
        {
            if (element.ValueKind != JsonValueKind.Object || !element.TryGetProperty(name, out JsonElement property))
            {
                yield break;
            }

            JsonElement array = property.ValueKind == JsonValueKind.Object
                && property.TryGetProperty("results", out JsonElement results)
                ? results
                : property;

            if (array.ValueKind != JsonValueKind.Array)
            {
                yield break;
            }

            foreach (JsonElement item in array.EnumerateArray())
            {
                yield return item;
            }
        }

        /// <summary>
        /// Wraps a collection the way verbose OData requires on the way <em>out</em>.
        /// </summary>
        internal static Dictionary<string, object> Collection(IEnumerable<string> values)
        {
            return new Dictionary<string, object> { ["results"] = new List<string>(values) };
        }

        internal static string StringOf(JsonElement element, string name)
        {
            return element.ValueKind == JsonValueKind.Object
                && element.TryGetProperty(name, out JsonElement value)
                && value.ValueKind == JsonValueKind.String
                ? value.GetString()
                : null;
        }

        internal static Guid GuidOf(JsonElement element, string name)
        {
            return Guid.TryParse(StringOf(element, name), out Guid id) ? id : Guid.Empty;
        }

        internal static bool BoolOf(JsonElement element, string name)
        {
            if (element.ValueKind != JsonValueKind.Object || !element.TryGetProperty(name, out JsonElement value))
            {
                return false;
            }

            // Verbose OData answers with a real boolean in some places and the string "true" in
            // others - DesignType and ListColor on a site design come back as strings.
            return value.ValueKind == JsonValueKind.True
                || (value.ValueKind == JsonValueKind.String
                    && bool.TryParse(value.GetString(), out bool parsed) && parsed);
        }
    }
}
