using System.Text.Json;

namespace PnP.Core
{
    /// <summary>
    /// Conversion to object extensions
    /// </summary>
    internal static class JsonExtensions
    {
        /// <summary>
        /// Deserializes a JsonElement to an Object
        /// </summary>
        internal static T ToObject<T>(this JsonElement element, JsonSerializerOptions options = null)
        {
            var json = element.GetRawText();
            return JsonSerializer.Deserialize<T>(json, options);
        }
    }

}