using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace PnP.Core.Test.Services.Core.CSOM.Utils.CustomConverters
{
    internal sealed class SPGuidConverter : JsonConverter<Guid>
    {
        public override Guid Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            // When for some reason the returned value is null instead of "/Guid(00000000-0000-0000-0000-000000000000)/" we'll
            // need to handle that as Guid.Empty
            if (reader.TokenType == JsonTokenType.String)
            {
                string value = reader.GetString();
                if (value != null && Guid.TryParse(value.Replace("/Guid(", "").Replace(")/", ""), out Guid result))
                {
                    return result;
                }
            }

            return Guid.Empty;
        }

        public override void Write(Utf8JsonWriter writer, Guid value, JsonSerializerOptions options)
        {
            writer.WriteStringValue("\\/Guid(" + value.ToString() + ")/");
        }
    }
}
