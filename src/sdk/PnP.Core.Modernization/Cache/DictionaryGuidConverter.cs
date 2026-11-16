using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace PnP.Core.Modernization.Cache
{
    internal class DictionaryGuidConverter : JsonConverter<Dictionary<Guid, string>>
    {
        public override Dictionary<Guid, string> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType != JsonTokenType.StartObject)
            {
                throw new JsonException();
            }

            var value = new Dictionary<Guid, string>();

            while (reader.Read())
            {
                if (reader.TokenType == JsonTokenType.EndObject)
                {
                    return value;
                }

                string keyString = reader.GetString();

                if (!Guid.TryParse(keyString, out Guid keyAsGuid))
                {
                    throw new JsonException($"Unable to convert \"{keyString}\" to System.Guid.");
                }

                reader.Read();

                string itemValue = reader.GetString();

                value.Add(keyAsGuid, itemValue);
            }

            throw new JsonException("Error Occured");
        }

        public override void Write(Utf8JsonWriter writer, Dictionary<Guid, string> value, JsonSerializerOptions options)
        {
            writer.WriteStartObject();

            foreach (KeyValuePair<Guid, string> item in value)
            {
                writer.WriteString(item.Key.ToString(), item.Value);
            }

            writer.WriteEndObject();
        }
    }
}
