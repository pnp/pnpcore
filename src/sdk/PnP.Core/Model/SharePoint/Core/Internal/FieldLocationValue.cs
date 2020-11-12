using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;

namespace PnP.Core.Model.SharePoint
{
    internal class FieldLocationValue : FieldValue, IFieldLocationValue
    {
        internal FieldLocationValue(string propertyName, TransientDictionary parent) : base(propertyName, parent)
        {
        }

        internal override string SharePointRestType => "";

        internal override Guid CsomType => Guid.Parse("97650aff-7e7b-44be-ac6e-d559f7f897a2");

        public string DisplayName { get => GetValue<string>(); set => SetValue(value); }

        public string LocationUri { get => GetValue<string>(); set => SetValue(value); }

        public string Street { get => GetValue<string>(); set => SetValue(value); }

        public string City { get => GetValue<string>(); set => SetValue(value); }

        public string State { get => GetValue<string>(); set => SetValue(value); }

        public string CountryOrRegion { get => GetValue<string>(); set => SetValue(value); }

        public string PostalCode { get => GetValue<string>(); set => SetValue(value); }

        public double Latitude { get => GetValue<double>(); set => SetValue(value); }

        public double Longitude { get => GetValue<double>(); set => SetValue(value); }

        internal override IFieldValue FromJson(JsonElement json)
        {
            throw new NotImplementedException();
        }

        internal override IFieldValue FromListDataAsStream(Dictionary<string, string> properties)
        {
            if (properties.ContainsKey("DisplayName"))
            {
                DisplayName = properties["DisplayName"];
            }

            if (properties.ContainsKey("LocationUri"))
            {
                LocationUri = properties["LocationUri"];
            }

            if (properties.ContainsKey("Street"))
            {
                Street = properties["Street"];
            }

            if (properties.ContainsKey("City"))
            {
                City = properties["City"];
            }

            if (properties.ContainsKey("State"))
            {
                State = properties["State"];
            }

            if (properties.ContainsKey("CountryOrRegion"))
            {
                CountryOrRegion = properties["CountryOrRegion"];
            }

            if (properties.ContainsKey("PostalCode"))
            {
                PostalCode = properties["PostalCode"];
            }

            if (properties.ContainsKey("Latitude"))
            {
                if (double.TryParse(properties["Latitude"], out double latitude))
                {
                    Latitude = latitude;
                }
            }

            if (properties.ContainsKey("Longitude"))
            {
                if (double.TryParse(properties["Longitude"], out double longitude))
                {
                    Longitude = longitude;
                }
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
