using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace PnP.Core
{
    internal class VersionConverter : JsonConverter<Version>
    {
        public override Version Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var stringVersion = reader.GetString();
            if (!string.IsNullOrEmpty(stringVersion))
            {
                return new Version(stringVersion);
            }
            return null;
        }

        public override void Write(Utf8JsonWriter writer, Version value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(value.ToString());
        }
    }
}
