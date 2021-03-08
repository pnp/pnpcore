using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;

namespace PnP.Core.Services
{
    internal static class CsomHelper
    {
        internal static string ListItemSpecialFieldProperty = "<Property Name=\"{FieldName}\" Type=\"{FieldType}\">{FieldValue}</Property>";
        internal static string ListItemSpecialFieldPropertyEmpty = "<Property Name=\"{FieldName}\" Type=\"{FieldType}\" />";
        internal static string ListItemTaxonomyMultiValueFieldIdentity = "<Identity Id=\"{TaxFieldIdentityObjectId}\" Name=\"86e78d9f-90e2-2000-915f-217ff0ac791d|740c6a0b-85e2-48a0-a494-e0f1759d4aa7:site:{Site.Id}:web:{Web.Id}:list:{Parent.Id}:field:{TaxonomyFieldId}\" />";

        // Taxonomy field creation
        internal static string TaxonomyFieldUpdate = "<Request AddExpandoFieldTypeSuffix=\"true\" SchemaVersion=\"15.0.0.0\" LibraryVersion=\"16.0.0.0\" ApplicationName=\"pnp core sdk\" xmlns=\"http://schemas.microsoft.com/sharepoint/clientquery/2009\"><Actions><SetProperty Id=\"{Counter}\" ObjectPathId=\"15\" Name=\"SspId\"><Parameter Type=\"Guid\">{TermStoreId}</Parameter></SetProperty><SetProperty Id=\"{Counter2}\" ObjectPathId=\"15\" Name=\"TermSetId\"><Parameter Type=\"Guid\">{TermSetId}</Parameter></SetProperty><SetProperty Id=\"{Counter3}\" ObjectPathId=\"15\" Name=\"TargetTemplate\"><Parameter Type=\"String\"></Parameter></SetProperty><SetProperty Id=\"{Counter4}\" ObjectPathId=\"15\" Name=\"AnchorId\"><Parameter Type=\"Guid\">{00000000-0000-0000-0000-000000000000}</Parameter></SetProperty><Method Name=\"Update\" Id=\"{Counter5}\" ObjectPathId=\"15\" /></Actions><ObjectPaths><Identity Id=\"15\" Name=\"1e1a939f-60b2-2000-98a6-d25d3d400a3a|740c6a0b-85e2-48a0-a494-e0f1759d4aa7:site:{Site.Id}:web:{Web.Id}{ListFieldId}:field:{Id}\" /></ObjectPaths></Request>";
        internal static string TaxonomyFieldListObjectId = ":list:{Parent.Id}";

        // ContentType creation
        internal static string ContentTypeCreate = "<Request xmlns=\"http://schemas.microsoft.com/sharepoint/clientquery/2009\" AddExpandoFieldTypeSuffix=\"true\" SchemaVersion=\"15.0.0.0\" LibraryVersion=\"16.0.0.0\" ApplicationName=\"pnp core sdk\"><Actions><ObjectPath Id=\"40\" ObjectPathId=\"39\" /><ObjectIdentityQuery Id=\"41\" ObjectPathId=\"39\" /></Actions><ObjectPaths><Method Id=\"39\" ParentId=\"5\" Name=\"Add\"><Parameters><Parameter TypeId=\"{168f3091-4554-4f14-8866-b20d48e45b54}\">{ContentTypeActualDescription}{ContentTypeActualGroup}<Property Name=\"Id\" Type=\"String\">{ContentTypeStringId}</Property><Property Name=\"Name\" Type=\"String\">{ContentTypeName}</Property><Property Name=\"ParentContentType\" Type=\"Null\" /></Parameter></Parameters></Method><Property Id=\"5\" ParentId=\"3\" Name=\"ContentTypes\" /><Property Id=\"3\" ParentId=\"1\" Name=\"Web\" /><StaticProperty Id=\"1\" TypeId=\"{3747adcd-a3c3-41b9-bfab-4a64dd2f1e0a}\" Name=\"Current\" /></ObjectPaths></Request>";

        // Properties to replace in above XML snippets
        internal static string Counter = "{Counter}";
        internal static string Counter2 = "{Counter2}";
        internal static string Counter3 = "{Counter3}";
        internal static string Counter4 = "{Counter4}";
        internal static string Counter5 = "{Counter5}";
        internal static string FieldValues = "{FieldValues}";
        internal static string ArrayValues = "{ArrayValues}";
        internal static string FieldName = "{FieldName}";
        internal static string FieldType = "{FieldType}";
        internal static string FieldValue = "{FieldValue}";
        internal static string ObjectId = "{ObjectId}";
        internal static string PropertyName = "{PropertyName}";
        internal static string TaxFieldIdentityObjectId = "{TaxFieldIdentityObjectId}";
        internal static string TaxFieldObjectId = "{TaxFieldObjectId}";
        internal static string TaxonomyFieldId = "{TaxonomyFieldId}";
        internal static string TaxonomyMultiValueIdentities = "{TaxonomyMultiValueIdentities}";
        internal static string TaxonomyMultiValueObjectPaths = "{TaxonomyMultiValueObjectPaths}";
        internal static string TermStoreId = "{TermStoreId}";
        internal static string TermSetId = "{TermSetId}";
        internal static string ListFieldId = "{ListFieldId}";
        internal static string ContentTypeActualDescription = "{ContentTypeActualDescription}";
        internal static string ContentTypeActualGroup = "{ContentTypeActualGroup}";
        internal static string ContentTypeStringId = "{ContentTypeStringId}";
        internal static string ContentTypeName = "{ContentTypeName}";

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
