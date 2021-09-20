using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// The IconAlignment property is used on collapsible sections and has string values left and right. However when no value is set 
    /// the property acts as bool property with value true
    /// </summary>
    internal class IconAlignmentJsonConverter : JsonConverter<string>
    {
        public override bool CanConvert(Type typeToConvert)
        {
            return (true);
        }

        public override string Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.True)
            {
                return "true";
            }
            else if (reader.TokenType == JsonTokenType.False)
            {
                return "false";
            }
            else
            {
                return reader.GetString();
            }
        }

        public override void Write(Utf8JsonWriter writer, string value, JsonSerializerOptions options)
        {
            if (value.Equals("true", StringComparison.InvariantCultureIgnoreCase))
            {
                writer.WriteBooleanValue(true);
            }
            else if (value.Equals("false", StringComparison.InvariantCultureIgnoreCase))
            {
                writer.WriteBooleanValue(false);
            }
            else
            {
                writer.WriteStringValue(value);
            }            
        }
    }
}
