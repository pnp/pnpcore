using PnP.Core.Services;
using System;
using System.Text;
using System.Text.Json;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// Base class for reading and writing of "special" field types like a lookup, user, url...
    /// </summary>
    internal class FieldUrlValue : FieldValue, IFieldUrlValue
    {
        internal FieldUrlValue(string propertyName, TransientDictionary parent) : base(propertyName, parent)
        {
        }

        internal override string SharePointRestType { get => "SP.FieldUrlValue"; }

        internal override Guid CsomType { get => Guid.Parse("fa8b44af-7b43-43f2-904a-bd319497011e"); }

        public string Url { get => GetValue<string>(); set => SetValue(value); }

        public string Description { get => GetValue<string>(); set => SetValue(value); }

        internal override IFieldValue FromJson(JsonElement json)
        {
            if (json.TryGetProperty(nameof(Url), out JsonElement url))
            {
                Url = url.GetString();
            }

            if (json.TryGetProperty(nameof(Description), out JsonElement description))
            {
                Description = description.GetString();
            }

            return this;
        }

        internal override object ToJson()
        {
            var updateMessage = new
            {
                __metadata = new { type = SharePointRestType },
                Url,
                Description
            };

            return updateMessage;
        }

        internal override string ToCsomXml()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(CsomHelper.ListItemSpecialFieldProperty.Replace(CsomHelper.FieldName, nameof(Url)).Replace(CsomHelper.FieldType, Url.GetType().Name).Replace(CsomHelper.FieldValue, Url));
            sb.Append(CsomHelper.ListItemSpecialFieldProperty.Replace(CsomHelper.FieldName, nameof(Description)).Replace(CsomHelper.FieldType, Description.GetType().Name).Replace(CsomHelper.FieldValue, Description));
            return sb.ToString();
        }
    }
}
