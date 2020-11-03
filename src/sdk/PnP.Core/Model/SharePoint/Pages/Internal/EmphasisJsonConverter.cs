using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace PnP.Core.Model.SharePoint
{
    internal class EmphasisJsonConverter : JsonConverter<int>
    {
        public override bool CanConvert(Type typeToConvert)
        {
            return (true);
        }

        public override int Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            object initialValue = reader.GetString();
            int result = 0;

            if (initialValue != null)
            {
                var stringValue = initialValue.ToString();
                if (!string.IsNullOrEmpty(stringValue) &&
                    stringValue.Equals("undefined", StringComparison.InvariantCultureIgnoreCase))
                {
                    result = 0;
                }
                else if (!int.TryParse(stringValue, out result))
                {
                    result = 0;
                }
            }

            return (result);
        }

        public override void Write(Utf8JsonWriter writer, int value, JsonSerializerOptions options)
        {
            writer.WriteNumberValue(value);
        }
    }
}
