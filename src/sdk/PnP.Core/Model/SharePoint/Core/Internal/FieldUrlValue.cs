using PnP.Core.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// Base class for reading and writing of "special" field types like a lookup, user, url...
    /// </summary>
    internal class FieldUrlValue : FieldValue, IFieldUrlValue
    {
        internal FieldUrlValue() : base()
        {
        }

        internal override string SharePointRestType { get => "SP.FieldUrlValue"; }

        internal override Guid CsomType { get => Guid.Parse("fa8b44af-7b43-43f2-904a-bd319497011e"); }

        public string Url { get => GetValue<string>(); set => SetValue(value); }

        public string Description { get => GetValue<string>(); set => SetValue(value); }

        internal override IFieldValue FromJson(JsonElement json)
        {
#pragma warning disable CA1507 // Use nameof to express symbol names
            if (json.TryGetProperty("Url", out JsonElement url))
#pragma warning restore CA1507 // Use nameof to express symbol names
            {
                Url = url.GetString();
            }

#pragma warning disable CA1507 // Use nameof to express symbol names
            if (json.TryGetProperty("Description", out JsonElement description))
#pragma warning restore CA1507 // Use nameof to express symbol names
            {
                Description = description.GetString();
            }

            if (!HasValue(nameof(Description)))
            {
                Description = Url;
            }

            return this;
        }

        internal override IFieldValue FromListDataAsStream(Dictionary<string, string> properties)
        {
            // empty url value => return null. Needed to have the same behaviour as when doing a GetAsync() call
            if (!properties.Any() || string.IsNullOrEmpty(properties.First().Value))
            {
                return null;
            }

            // first property is the url field
            Url = properties.First().Value;

            if (properties.ContainsKey("desc"))
            {
                Description = properties["desc"];
            }

            if (!HasValue(nameof(Description)) && HasValue(nameof(Url)))
            {
                Description = Url;
            }

            return this;
        }

        internal override object ToJson()
        {
            if (!HasValue(nameof(Url)))
            {
                return "";
            }

            var updateMessage = new
            {
                __metadata = new { type = SharePointRestType },
                Url,
                Description
            };

            return updateMessage;
        }

        internal override object ToValidateUpdateItemJson()
        {
            if (!HasValue(nameof(Url)))
            {
                return "";
            }

            string fieldValue = $"{Url}";
            if (HasValue(nameof(Description)) && !string.IsNullOrEmpty(Description))
            {
                fieldValue += $", {Description}";
            }

            return fieldValue;
        }

        internal override string ToCsomXml()
        {
            if (!HasValue(nameof(Url)))
            {
                return "";
            }

            StringBuilder sb = new StringBuilder();
            sb.Append(CsomHelper.ListItemSpecialFieldProperty
                .Replace(CsomHelper.FieldName, "Url")
                .Replace(CsomHelper.FieldType, Url.GetType().Name)
                .Replace(CsomHelper.FieldValue, CsomHelper.XmlString(Url)));
            sb.Append(CsomHelper.ListItemSpecialFieldProperty
                .Replace(CsomHelper.FieldName, "Description")
                .Replace(CsomHelper.FieldType, Description.GetType().Name)
                .Replace(CsomHelper.FieldValue, CsomHelper.XmlString(Description)));
            return sb.ToString();
        }
    }
}
