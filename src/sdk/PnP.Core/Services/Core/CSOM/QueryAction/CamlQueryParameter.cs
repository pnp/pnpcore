using PnP.Core.Model.SharePoint;
using System.Text;

namespace PnP.Core.Services.Core.CSOM.QueryAction
{
    /// <summary>
    /// Serializes an SP.CamlQuery method parameter, including the nested SP.ListItemCollectionPosition property
    /// which the generic <see cref="Parameter"/> serialization cannot express
    /// </summary>
    internal sealed class CamlQueryParameter : Parameter
    {
        private readonly CamlQueryOptions queryOptions;

        internal CamlQueryParameter(CamlQueryOptions queryOptions)
        {
            // TypeId of SP.CamlQuery
            TypeId = "{3d248d7b-fc86-40a3-aa97-02a75d69fb8a}";
            this.queryOptions = queryOptions;
        }

        internal override string SerializeParameter()
        {
            StringBuilder sb = new StringBuilder();

            // Properties in the order the CSOM client serializes them
            sb.Append($"<{ParameterTagName} TypeId=\"{TypeId}\">");

            AppendBoolProperty(sb, "AllowIncrementalResults", queryOptions.AllowIncrementalResults);
            AppendBoolProperty(sb, "DatesInUtc", queryOptions.DatesInUtc);
            AppendStringProperty(sb, "FolderServerRelativeUrl", queryOptions.FolderServerRelativeUrl);

            if (!string.IsNullOrEmpty(queryOptions.PagingInfo))
            {
                // TypeId of SP.ListItemCollectionPosition
                sb.Append("<Property Name=\"ListItemCollectionPosition\" TypeId=\"{922354eb-c56a-4d88-ad59-67496854efe1}\">");
                AppendStringProperty(sb, "PagingInfo", queryOptions.PagingInfo);
                sb.Append("</Property>");
            }
            else
            {
                sb.Append("<Property Name=\"ListItemCollectionPosition\" Type=\"Null\" />");
            }

            AppendStringProperty(sb, "ViewXml", queryOptions.ViewXml);

            sb.Append($"</{ParameterTagName}>");

            return sb.ToString();
        }

        private static void AppendBoolProperty(StringBuilder sb, string name, bool? value)
        {
            if (value.HasValue)
            {
                sb.Append($"<Property Name=\"{name}\" Type=\"Boolean\">{value.Value.ToString().ToLowerInvariant()}</Property>");
            }
            else
            {
                sb.Append($"<Property Name=\"{name}\" Type=\"Null\" />");
            }
        }

        private static void AppendStringProperty(StringBuilder sb, string name, string value)
        {
            if (value != null)
            {
                sb.Append($"<Property Name=\"{name}\" Type=\"String\">{CsomHelper.XmlString(value)}</Property>");
            }
            else
            {
                sb.Append($"<Property Name=\"{name}\" Type=\"Null\" />");
            }
        }
    }
}
