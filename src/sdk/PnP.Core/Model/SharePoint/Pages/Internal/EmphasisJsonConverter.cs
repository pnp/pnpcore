using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace PnP.Core.Model.SharePoint
{
    internal sealed class EmphasisJsonConverter : JsonConverter<int>
    {
        public override bool CanConvert(Type typeToConvert)
        {
            return (true);
        }

        public override int Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.Number && reader.TryGetInt32(out int intValue))
            {
                return intValue;
            }
            else
            {
                var stringValue = reader.GetString();

                int result;
                if (!string.IsNullOrEmpty(stringValue) &&
                    stringValue.Equals("undefined", StringComparison.InvariantCultureIgnoreCase))
                {
                    result = 0;
                }
                else if (!int.TryParse(stringValue, out result))
                {
                    result = 0;
                }

                return (result);
            }
        }

        public override void Write(Utf8JsonWriter writer, int value, JsonSerializerOptions options)
        {
            writer.WriteNumberValue(value);
        }
    }
}
