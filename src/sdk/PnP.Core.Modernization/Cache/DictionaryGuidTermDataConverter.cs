using PnP.Core.Modernization.Entities;
using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace PnP.Core.Modernization.Cache
{
    internal class DictionaryGuidTermDataConverter : JsonConverter<Dictionary<Guid, TermData>>
    {
        public override Dictionary<Guid, TermData> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType != JsonTokenType.StartObject)
            {
                throw new JsonException();
            }

            var value = new Dictionary<Guid, TermData>();

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

                var termData = JsonSerializer.Deserialize<TermData>(itemValue);

                value.Add(keyAsGuid, termData);
            }

            throw new JsonException("Error Occured");
        }

        public override void Write(Utf8JsonWriter writer, Dictionary<Guid, TermData> value, JsonSerializerOptions options)
        {
            writer.WriteStartObject();

            foreach (KeyValuePair<Guid, TermData> item in value)
            {
                var termDataString = JsonSerializer.Serialize(item.Value);

                writer.WriteString(item.Key.ToString(), termDataString);
            }

            writer.WriteEndObject();
        }
    }
}
