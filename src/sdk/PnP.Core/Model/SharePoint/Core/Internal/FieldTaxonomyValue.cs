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

        internal override string SharePointRestType => "TaxonomyFieldValue:#Microsoft.SharePoint.Taxonomy";

        internal override Guid CsomType => Guid.Parse("19e70ed0-4177-456b-8156-015e4d163ff8");

        public string Label { get => GetValue<string>(); set => SetValue(value); }

        public Guid TermId { get => GetValue<Guid>(); set => SetValue(value); }

        internal override IFieldValue FromJson(JsonElement json)
        {
            throw new NotImplementedException();
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

            return this;
        }

        internal override string ToCsomXml()
        {
            throw new NotImplementedException();
        }

        internal override object ToJson()
        {
            throw new NotImplementedException();
        }
    }
}
