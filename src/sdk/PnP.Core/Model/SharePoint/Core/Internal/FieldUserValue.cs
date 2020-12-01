using PnP.Core.Model.Security;
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

        public ISharePointPrincipal Principal
        {
            get
            {
                return GetValue<ISharePointPrincipal>();
            }
            set
            { 
                SetValue(value);

                if (value != null)
                {
                    SetValue(value.Id, nameof(LookupId));
                }
                else
                {
                    SetValue(-1, nameof(LookupId));
                }
            }
        }

        public string Sip { get => GetValue<string>(); set => SetValue(value); }

        public string Email { get => GetValue<string>(); set => SetValue(value); }

        public string Title { get => GetValue<string>(); set => SetValue(value); }

        public string Picture { get => GetValue<string>(); set => SetValue(value); }

        internal override IFieldValue FromJson(JsonElement json)
        {
            if (json.ValueKind == JsonValueKind.String)
            {
                LookupId = int.Parse(json.GetString());
            } 
            else if (json.ValueKind == JsonValueKind.Number)
            {
                LookupId = json.GetInt32();
            }
            else
            {
                LookupId = -1;
            }

            return this;
        }

        internal override IFieldValue FromListDataAsStream(Dictionary<string, string> properties)
        {
            if (properties.ContainsKey("id"))
            {
                if (string.IsNullOrEmpty(properties["id"]))
                {
                    LookupId = -1;
                    return this;
                }

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
                Sip = properties["sip"];
            }

            if (properties.ContainsKey("picture"))
            {
                Picture = properties["picture"];
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

        internal override object ToValidateUpdateItemJson()
        {
            if (Principal == null)
            {
                throw new ClientException(ErrorType.Unsupported, PnPCoreResources.Exception_Unsupported_MissingSharePointPrincipal);
            }

            var users = new List<object>
            {
                new { Key = Principal.LoginName }
            };

            return JsonSerializer.Serialize(users.ToArray());
        }

        internal override string ToCsomXml()
        {
            if (!HasValue(nameof(LookupId)))
            {
                return "";
            }

            /*
            <Parameter TypeId="{c956ab54-16bd-4c18-89d2-996f57282a6f}">
                <Property Name="Email" Type="Null" />
                <Property Name="LookupId" Type="Int32">6</Property>
                <Property Name="LookupValue" Type="Null" />
            </Parameter>
             */
            StringBuilder sb = new StringBuilder();
            sb.Append(CsomHelper.ListItemSpecialFieldPropertyEmpty
                .Replace(CsomHelper.FieldName, "Email")
                .Replace(CsomHelper.FieldType, "Null"));
            sb.Append(CsomHelper.ListItemSpecialFieldProperty
                .Replace(CsomHelper.FieldName, "LookupId")
                .Replace(CsomHelper.FieldType, LookupId.GetType().Name)
                .Replace(CsomHelper.FieldValue, LookupId.ToString()));
            sb.Append(CsomHelper.ListItemSpecialFieldPropertyEmpty
                .Replace(CsomHelper.FieldName, "LookupValue")
                .Replace(CsomHelper.FieldType, "Null"));
            return sb.ToString();
        }
    }
}
