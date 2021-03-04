using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace PnP.Core.Test.Services.Core.CSOM.Utils.CustomConverters
{
    internal class SPGuidConverter : JsonConverter<Guid>
    {
        public override Guid Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            string value = reader.GetString();
            if (Guid.TryParse(value.Replace("/Guid(", "").Replace(")/", ""), out Guid result))
            {
                return result;
            }
            return Guid.Empty;

        }

        public override void Write(Utf8JsonWriter writer, Guid value, JsonSerializerOptions options)
        {
            writer.WriteStringValue("\\/Guid(" + value.ToString() + ")/");
        }
    }
}
