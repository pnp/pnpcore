using PnP.Core.Services.Core.CSOM;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;

namespace PnP.Core.Model.SharePoint
{
    internal class FieldLookupValue : FieldValue, IFieldLookupValue
    {
        internal FieldLookupValue() : base()
        {
        }

        internal override string SharePointRestType => "";

        internal override Guid CsomType => Guid.Parse("f1d34cc0-9b50-4a78-be78-d5facfcccfb7");

        public int LookupId
        {
            get
            {
                if (!HasValue())
                {
                    return -1;
                }

                return GetValue<int>();
            }

            set => SetValue(value);
        }

        public string LookupValue { get => GetValue<string>(); set => SetValue(value); }

        public bool IsSecretFieldValue { get => GetValue<bool>(); set => SetValue(value); }

        internal override IFieldValue FromJson(JsonElement json)
        {
            if (json.ValueKind == JsonValueKind.Number)
            {
                LookupId = json.GetInt32();

                // Clear changes
                Commit();

                return this;
            }
            else
            {
                return null;
            }
        }

        internal override IFieldValue FromListDataAsStream(Dictionary<string, string> properties)
        {
            if (!properties.ContainsKey("lookupId"))
            {
                return null;
            }

            if (properties.ContainsKey("lookupId"))
            {
                LookupId = int.Parse(properties["lookupId"]);
            }

            if (properties.ContainsKey("lookupValue"))
            {
                LookupValue = properties["lookupValue"];
            }

            if (properties.ContainsKey("isSecretFieldValue"))
            {
                IsSecretFieldValue = bool.Parse(properties["isSecretFieldValue"]);
            }

            // Clear changes
            Commit();

            return this;
        }

        internal override object ToJson()
        {
            var updateMessage = new
            {
                LookupId
            };

            return updateMessage;
        }

        internal override object ToValidateUpdateItemJson()
        {
            return $"{LookupId}";
        }

        internal override string ToCsomXml()
        {
            if (!HasValue(nameof(LookupId)))
            {
                return "";
            }

            StringBuilder sb = new StringBuilder();
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
