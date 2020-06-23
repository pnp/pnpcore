using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace PnP.Core.Services
{
    internal class SharePointRestCollectionJsonConverter<TItem> : JsonConverter<TItem[]>
    {
        public override TItem[] Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            // No need to deserialize for now
            throw new NotImplementedException();
        }

        public override void Write(Utf8JsonWriter writer, TItem[] value, JsonSerializerOptions options)
        {
            writer.WriteStartObject();
            writer.WritePropertyName("results");
            writer.WriteStartArray();
            foreach (var item in value)
            {
                JsonSerializer.Serialize(writer, item, options);
            }
            writer.WriteEndArray();
            writer.WriteEndObject();
        }
    }
}
