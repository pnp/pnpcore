using System;
using System.Linq;
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

        /// <summary>
        /// Deserializes a JsonElement to an Object based on the provided type
        /// </summary>
        internal static object ToObject(this JsonElement element, Type type, JsonSerializerOptions options = null)
        {
            var json = element.GetRawText();
            return JsonSerializer.Deserialize(json, type, options);
        }

        /// <summary>
        /// Gets child JsonElement by it's name. If there is no child element with such name, returns <see cref="Nullable{JsonElement}" />
        /// </summary>
        internal static JsonElement? Get(this JsonElement element, string name) =>
        element.ValueKind != JsonValueKind.Null && element.ValueKind != JsonValueKind.Undefined && element.TryGetProperty(name, out var value)
            ? value : (JsonElement?)null;

        /// <summary>
        /// Gets child JsonElement by it's index. If there is no child element with such name, returns <see cref="Nullable{JsonElement}" />
        /// </summary>
        internal static JsonElement? Get(this JsonElement element, int index)
        {
            if (element.ValueKind == JsonValueKind.Null || element.ValueKind == JsonValueKind.Undefined)
                return null;
            var value = element.EnumerateArray().ElementAtOrDefault(index);
            return value.ValueKind != JsonValueKind.Undefined ? value : (JsonElement?)null;
        }
    }

}