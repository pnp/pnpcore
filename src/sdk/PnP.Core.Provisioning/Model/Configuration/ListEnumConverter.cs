using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace PnP.Core.Provisioning.Model.Configuration
{
    public class ListEnumConverter<TEnum> : JsonConverter<List<TEnum>> where TEnum : struct, Enum
    {
        public override List<TEnum> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var list = new List<TEnum>();
            while (reader.Read())
            {
                if(reader.TokenType == JsonTokenType.EndArray)
                {
                    return list;
                }
                if (reader.TokenType == JsonTokenType.String)
                {
                    var value = reader.GetString();
                    if (Enum.TryParse(value, out TEnum enumvalue))
                    {
                        list.Add(enumvalue);
                    }
                }
            }
            return null;
        }

        public override void Write(Utf8JsonWriter writer, List<TEnum> value, JsonSerializerOptions options)
        {
            throw new NotImplementedException();
        }
    }
}
