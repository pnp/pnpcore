using System;
using System.Collections.Generic;
using System.Text.Json;

namespace PnP.Core.Model.SharePoint
{
    internal class FieldTaxonomyValue : FieldValue, IFieldTaxonomyValue
    {
        internal FieldTaxonomyValue(string propertyName, TransientDictionary parent) : base(propertyName, parent)
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

        internal override string ToCsomXml()
        {
            throw new NotImplementedException();
        }

        public void LoadTerm(Guid termId, string label, int wssId = -1)
        {
            TermId = termId;
            Label = label;
            WssId = wssId;
        }
    }
}
