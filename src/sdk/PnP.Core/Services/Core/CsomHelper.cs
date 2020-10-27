using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;

namespace PnP.Core.Services
{
    internal static class CsomHelper
    {
        internal static string ListItemSystemUpdate = "<Request AddExpandoFieldTypeSuffix=\"true\" SchemaVersion=\"15.0.0.0\" LibraryVersion=\"16.0.0.0\" ApplicationName=\"pnp core sdk\" xmlns=\"http://schemas.microsoft.com/sharepoint/clientquery/2009\"><Actions>{FieldValues}<Method Name=\"SystemUpdate\" Id=\"{Counter}\" ObjectPathId=\"7\" /></Actions><ObjectPaths><Identity Id=\"7\" Name=\"121a659f-e03e-2000-4281-1212829d67dd|740c6a0b-85e2-48a0-a494-e0f1759d4aa7:site:{Site.Id}:web:{Web.Id}:list:{Parent.Id}:item:{Id},1\" /></ObjectPaths></Request>";
        internal static string ListItemSystemUpdateSetFieldValue = "<Method Name=\"SetFieldValue\" Id=\"{Counter}\" ObjectPathId=\"7\"><Parameters><Parameter Type=\"String\">{FieldName}</Parameter><Parameter Type=\"{FieldType}\">{FieldValue}</Parameter></Parameters></Method>";
        internal static string ListItemUpdateOverwriteVersion = "<Request AddExpandoFieldTypeSuffix=\"true\" SchemaVersion=\"15.0.0.0\" LibraryVersion=\"16.0.0.0\" ApplicationName=\"pnp core sdk\" xmlns=\"http://schemas.microsoft.com/sharepoint/clientquery/2009\"><Actions>{FieldValues}<Method Name=\"UpdateOverwriteVersion\" Id=\"{Counter}\" ObjectPathId=\"7\" /></Actions><ObjectPaths><Identity Id=\"7\" Name=\"121a659f-e03e-2000-4281-1212829d67dd|740c6a0b-85e2-48a0-a494-e0f1759d4aa7:site:{Site.Id}:web:{Web.Id}:list:{Parent.Id}:item:{Id},1\" /></ObjectPaths></Request>";

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
