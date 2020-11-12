using System;
using System.Collections.Generic;
using System.Text.Json;

namespace PnP.Core.Model.SharePoint
{
    internal class FieldLookupValue : FieldValue, IFieldLookupValue
    {
        internal FieldLookupValue(string propertyName, TransientDictionary parent) : base(propertyName, parent)
        {
        }

        internal override string SharePointRestType => "";

        internal override Guid CsomType => Guid.Parse("f1d34cc0-9b50-4a78-be78-d5facfcccfb7");

        public int LookupId { get => GetValue<int>(); set => SetValue(value); }

        public string LookupValue { get => GetValue<string>(); set => SetValue(value); }

        public bool IsSecretFieldValue { get => GetValue<bool>(); set => SetValue(value); }

        internal override IFieldValue FromJson(JsonElement json)
        {
            throw new NotImplementedException();
        }

        internal override IFieldValue FromListDataAsStream(Dictionary<string, string> properties)
        {
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

            return this;
        }

        internal override object ToJson()
        {
            throw new NotImplementedException();
        }

        internal override string ToCsomXml()
        {
            throw new NotImplementedException();
        }
    }
}
