using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace PnP.Core.Services.Core
{
    /// <summary>
    /// Used for Attribute addedFromPersistedData in WebPartControlData as it can be bool or string
    /// </summary>
    internal sealed class BoolJsonConverter : JsonConverter<bool>
    {
        public override bool Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            bool result;
            bool.TryParse(System.Text.Encoding.Default.GetString(reader.ValueSpan.ToArray()), out result);
            return result;
        }

        public override void Write(Utf8JsonWriter writer, bool value, JsonSerializerOptions options)
        {
            writer.WriteBooleanValue(value);
        }
    }
}
