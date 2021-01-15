using PnP.Core.Services;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;

namespace PnP.Core.Model.SharePoint
{
    internal class FieldTaxonomyValue : FieldValue, IFieldTaxonomyValue
    {
        internal FieldTaxonomyValue() : base()
        {
        }

        internal override string SharePointRestType => "SP.Taxonomy.TaxonomyFieldValue";

        internal override Guid CsomType => Guid.Parse("19e70ed0-4177-456b-8156-015e4d163ff8");

        public string Label { get => GetValue<string>(); set => SetValue(value); }

        public Guid TermId { get => GetValue<Guid>(); set => SetValue(value); }

        public int WssId { get => GetValue<int>(); set => SetValue(value); }

        internal override IFieldValue FromJson(JsonElement json)
        {
            if (json.TryGetProperty("TermGuid", out JsonElement termGuid))
            {
                TermId = termGuid.GetGuid();
            }

#pragma warning disable CA1507 // Use nameof to express symbol names
            if (json.TryGetProperty("Label", out JsonElement label))
#pragma warning restore CA1507 // Use nameof to express symbol names
            {
                Label = label.GetString();
            }

#pragma warning disable CA1507 // Use nameof to express symbol names
            if (json.TryGetProperty("WssId", out JsonElement wssId))
#pragma warning restore CA1507 // Use nameof to express symbol names
            {
                WssId = wssId.GetInt32();
            }

            return this;
        }

        internal override IFieldValue FromListDataAsStream(Dictionary<string, string> properties)
        {
            if (!properties.ContainsKey("TermID"))
            {
                return null;
            }
            
            if (properties.ContainsKey("Label"))
            {
                Label = properties["Label"];
            }

            if (properties.ContainsKey("TermID"))
            {
                TermId = Guid.Parse(properties["TermID"]);
            }

            WssId = -1;

            return this;
        }

        internal override object ToJson()
        {
            var updateMessage = new
            {
                __metadata = new { type = SharePointRestType },
                Label,
                TermGuid = TermId,
                WssId
            };

            return updateMessage;
        }

        internal override object ToValidateUpdateItemJson()
        {
            if (!HasValue(nameof(TermId)) || !HasValue(nameof(Label)))
            {
                return "";
            }

            return $"{Label}|{TermId};";
        }

        internal override string ToCsomXml()
        {
            if (!HasValue(nameof(TermId)) || !HasValue(nameof(Label)))
            {
                return "";
            }

            /*
            <Parameter TypeId="{19e70ed0-4177-456b-8156-015e4d163ff8}">
                <Property Name="Label" Type="String">MBI</Property>
                <Property Name="TermGuid" Type="String">1824510b-00e1-40ac-8294-528b1c9421e0</Property>
                <Property Name="WssId" Type="Int32">-1</Property>
            </Parameter>
             */
            StringBuilder sb = new StringBuilder();
            sb.Append(CsomHelper.ListItemSpecialFieldProperty
                .Replace(CsomHelper.FieldName, "Label")
                .Replace(CsomHelper.FieldType, Label.GetType().Name)
                .Replace(CsomHelper.FieldValue, Label != null ? CsomHelper.XmlString(Label) : "Null"));
            sb.Append(CsomHelper.ListItemSpecialFieldProperty
                .Replace(CsomHelper.FieldName, "TermGuid")
                .Replace(CsomHelper.FieldType, "String")
                .Replace(CsomHelper.FieldValue, TermId.ToString()));
            sb.Append(CsomHelper.ListItemSpecialFieldProperty
                .Replace(CsomHelper.FieldName, "WssId")
                .Replace(CsomHelper.FieldType, WssId.GetType().Name)
                .Replace(CsomHelper.FieldValue, WssId.ToString()));
            return sb.ToString();
        }

    }
}
