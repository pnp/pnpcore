using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;

namespace PnP.Core.Services.Core.CSOM
{
    internal static class CsomHelper
    {
        internal static string ListItemSpecialFieldProperty = "<Property Name=\"{FieldName}\" Type=\"{FieldType}\">{FieldValue}</Property>";
        internal static string ListItemSpecialFieldPropertyEmpty = "<Property Name=\"{FieldName}\" Type=\"{FieldType}\" />";

        // Properties to replace in above XML snippets
        internal static string FieldName = "{FieldName}";
        internal static string FieldType = "{FieldType}";
        internal static string FieldValue = "{FieldValue}";

        internal static Dictionary<int, JsonElement> ParseResponse(string jsonResponse)
        {
            Dictionary<int, JsonElement> responses = new Dictionary<int, JsonElement>();

            bool first = true;
            int nextActionId = 1;
            var responseJson = JsonSerializer.Deserialize<JsonElement>(jsonResponse);

            foreach (var response in responseJson.EnumerateArray())
            {
                if (first)
                {
                    first = false;
                    responses.Add(0, response);
                }
                else
                {
                    if (response.ValueKind == JsonValueKind.Number)
                    {
                        nextActionId = response.GetInt32();
                    }
                    else if (response.ValueKind == JsonValueKind.Object)
                    {
                        responses.Add(nextActionId, response);
                    }
                }
            }

            return responses;
        }

        internal static string XmlString(string text, bool isAttribute = false)
        {
            var sb = new StringBuilder(text.Length);

            foreach (var chr in text)
            {
                if (chr == '<')
                    sb.Append("&lt;");
                else if (chr == '>')
                    sb.Append("&gt;");
                else if (chr == '&')
                    sb.Append("&amp;");

                // special handling for quotes
                else if (isAttribute && chr == '\"')
                    sb.Append("&quot;");
                else if (isAttribute && chr == '\'')
                    sb.Append("&apos;");

                // Legal sub-chr32 characters
                else if (chr == '\n')
                    sb.Append(isAttribute ? "&#xA;" : "\n");
                else if (chr == '\r')
                    sb.Append(isAttribute ? "&#xD;" : "\r");
                else if (chr == '\t')
                    sb.Append(isAttribute ? "&#x9;" : "\t");

                else
                {
                    if (chr < 32)
                    {
                        throw new InvalidOperationException(string.Format(PnPCoreResources.Exception_Xml_InvalidXmlCharacter, Convert.ToInt16(chr)));
                    }
                    sb.Append(chr);
                }
            }

            return sb.ToString();
        }

    }
}
