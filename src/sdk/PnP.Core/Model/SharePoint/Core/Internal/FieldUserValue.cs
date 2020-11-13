using PnP.Core.Services;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;

namespace PnP.Core.Model.SharePoint
{
    internal class FieldUserValue : FieldLookupValue, IFieldUserValue
    {
        internal FieldUserValue(string propertyName, TransientDictionary parent) : base(propertyName, parent)
        {
        }

        internal override string SharePointRestType { get => "SP.FieldUserValue"; }

        internal override Guid CsomType { get => Guid.Parse("c956ab54-16bd-4c18-89d2-996f57282a6f"); }

        public string Email { get => GetValue<string>(); set => SetValue(value); }

        public string Title { get => GetValue<string>(); set => SetValue(value); }

        public string Sip { get => GetValue<string>(); set => SetValue(value); }

        public string Picture { get => GetValue<string>(); set => SetValue(value); }

        internal override IFieldValue FromJson(JsonElement json)
        {
            LookupId = int.Parse(json.GetString());
            return this;
        }

        internal override IFieldValue FromListDataAsStream(Dictionary<string, string> properties)
        {
            if (properties.ContainsKey("id"))
            {
                LookupId = int.Parse(properties["id"]);
            }

            if (properties.ContainsKey("email"))
            {
                Email = properties["email"];
            }

            if (properties.ContainsKey("title"))
            {
                Title = properties["title"];
            }

            if (properties.ContainsKey("sip"))
            {
                Title = properties["sip"];
            }

            if (properties.ContainsKey("picture"))
            {
                Title = properties["picture"];
            }

            return this;
        }

        internal override object ToJson()
        {
            var updateMessage = new
            {
                __metadata = new { type = SharePointRestType },
                LookupId,
            };

            return updateMessage;
        }

        internal override string ToCsomXml()
        {
            /*
            <Parameter TypeId="{c956ab54-16bd-4c18-89d2-996f57282a6f}">
                <Property Name="Email" Type="Null" />
                <Property Name="LookupId" Type="Int32">6</Property>
                <Property Name="LookupValue" Type="Null" />
            </Parameter>
             */
            StringBuilder sb = new StringBuilder();
            sb.Append(CsomHelper.ListItemSpecialFieldProperty.Replace(CsomHelper.FieldName, nameof(Email)).Replace(CsomHelper.FieldType, "Null").Replace(CsomHelper.FieldValue, ""));
            sb.Append(CsomHelper.ListItemSpecialFieldProperty.Replace(CsomHelper.FieldName, nameof(LookupId)).Replace(CsomHelper.FieldType, LookupId.GetType().Name).Replace(CsomHelper.FieldValue, LookupId.ToString()));
            sb.Append(CsomHelper.ListItemSpecialFieldProperty.Replace(CsomHelper.FieldName, nameof(Title)).Replace(CsomHelper.FieldType, "Null").Replace(CsomHelper.FieldValue, ""));
            return sb.ToString();
        }
    }
}
