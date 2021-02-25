using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace PnP.Core.Test.Services.Core.CSOM.Utils.CustomConverters
{
    class SPGuidConverter : JsonConverter<Guid>
    {
        public override Guid Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            string value = reader.GetString();
            Guid result;
            if (Guid.TryParse(value.Replace("/Guid(", "").Replace(")/", ""), out result))
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
