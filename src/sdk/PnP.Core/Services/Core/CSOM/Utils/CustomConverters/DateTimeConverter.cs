using PnP.Core.Services.Core.CSOM.Utils.DateHelpers;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace PnP.Core.Services.Core.CSOM.Utils.CustomConverters
{
    class DateTimeConverter : JsonConverter<DateTime>
    {
        public CSOMDateConverter DateConverter = new CSOMDateConverter();
        public override DateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            string value = reader.GetString();
            //ok, so far I found two different date formats returned from the endpoint
            //You can check out both in \PnP.Core.Test\Services\Core\CSOM\Utils\CSOMResponseHelperTests.cs
            //Hence we need two strategies
            DateTime? result =  DateConverter.ConverDate(value);

            return result.HasValue ? result.Value : DateTime.MinValue;
        }

        public override void Write(Utf8JsonWriter writer, DateTime value, JsonSerializerOptions options)
        {
            writer.WriteStringValue("\\/Date(" + value.ToString() + ")/");
        }
    }
}
