using System;
using System.Collections.Generic;
using System.Text.Json;

namespace PnP.Core.Model.SharePoint
{
    internal class FieldLocationValue : FieldValue, IFieldLocationValue
    {
        internal FieldLocationValue() : base()
        {
        }

        internal override string SharePointRestType => "SP.FieldGeolocationValue";

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

#pragma warning disable CA1507 // Use nameof to express symbol names
            if (json.TryGetProperty("LocationUri", out JsonElement locationUri))
#pragma warning restore CA1507 // Use nameof to express symbol names
            {
                LocationUri = locationUri.GetString();
            }

            if (json.TryGetProperty("Address", out JsonElement address))
            {
#pragma warning disable CA1507 // Use nameof to express symbol names
                if (address.TryGetProperty("Street", out JsonElement street))
#pragma warning restore CA1507 // Use nameof to express symbol names
                {
                    Street = street.GetString();
                }

#pragma warning disable CA1507 // Use nameof to express symbol names
                if (address.TryGetProperty("City", out JsonElement city))
#pragma warning restore CA1507 // Use nameof to express symbol names
                {
                    City = city.GetString();
                }

#pragma warning disable CA1507 // Use nameof to express symbol names
                if (address.TryGetProperty("State", out JsonElement state))
#pragma warning restore CA1507 // Use nameof to express symbol names
                {
                    State = state.GetString();
                }

#pragma warning disable CA1507 // Use nameof to express symbol names
                if (address.TryGetProperty("CountryOrRegion", out JsonElement countryOrRegion))
#pragma warning restore CA1507 // Use nameof to express symbol names
                {
                    CountryOrRegion = countryOrRegion.GetString();
                }

#pragma warning disable CA1507 // Use nameof to express symbol names
                if (address.TryGetProperty("PostalCode", out JsonElement postalCode))
#pragma warning restore CA1507 // Use nameof to express symbol names
                {
                    PostalCode = postalCode.GetString();
                }
            }

            if (json.TryGetProperty("Coordinates", out JsonElement coordinates))
            {
#pragma warning disable CA1507 // Use nameof to express symbol names
                if (coordinates.TryGetProperty("Latitude", out JsonElement latitude))
#pragma warning restore CA1507 // Use nameof to express symbol names
                {
                    if (latitude.ValueKind == JsonValueKind.Number)
                    {
                        Latitude = latitude.GetDouble();
                    }
                }

#pragma warning disable CA1507 // Use nameof to express symbol names
                if (coordinates.TryGetProperty("Longitude", out JsonElement longitude))
#pragma warning restore CA1507 // Use nameof to express symbol names
                {
                    if (longitude.ValueKind == JsonValueKind.Number)
                    {
                        Longitude = longitude.GetDouble();
                    }
                }
            }

            return this;
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

        internal override object ToJson()
        {
            var updateMessage = new
            {
                LocationUri,
                Coordinates = new
                {
                    Latitude,
                    Longitude
                }
            };

            return updateMessage;
        }

        internal override object ToValidateUpdateItemJson()
        {
            throw new NotImplementedException();
        }

        internal override string ToCsomXml()
        {
            throw new NotImplementedException();
        }

    }
}
