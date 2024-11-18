using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace PnP.Core.Model.SharePoint
{
    internal sealed class IndexJsonConverter : JsonConverter<float>
    {
        public override bool CanConvert(Type typeToConvert)
        {
            return (true);
        }

        public override float Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.Null)
            {
                return 0;
            }
            else
            {
                return reader.TryGetDouble(out double value) ? (float)value : 0;
            }
        }

        public override void Write(Utf8JsonWriter writer, float value, JsonSerializerOptions options)
        {
            writer.WriteNumberValue(value);
        }
    }
}
