using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Text.Json;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// Represents a location field value
    /// </summary>
    public sealed class FieldLocationValue : FieldValue, IFieldLocationValue
    {
        /// <summary>
        /// Default constructor
        /// </summary>
        public FieldLocationValue() : base()
        {
        }

        internal override string SharePointRestType => "SP.FieldGeolocationValue";

        internal override Guid CsomType => Guid.Parse("97650aff-7e7b-44be-ac6e-d559f7f897a2");

        /// <summary>
        /// Name identifiying this location
        /// </summary>
        public string DisplayName { get => GetValue<string>(); internal set => SetValue(value); }

        /// <summary>
        /// Uri identifying this location
        /// </summary>
        public string LocationUri { get => GetValue<string>(); internal set => SetValue(value); }

        /// <summary>
        /// Streetname
        /// </summary>
        public string Street { get => GetValue<string>(); internal set => SetValue(value); }

        /// <summary>
        /// City
        /// </summary>
        public string City { get => GetValue<string>(); internal set => SetValue(value); }

        /// <summary>
        /// State
        /// </summary>
        public string State { get => GetValue<string>(); internal set => SetValue(value); }

        /// <summary>
        /// Country of region
        /// </summary>
        public string CountryOrRegion { get => GetValue<string>(); internal set => SetValue(value); }

        /// <summary>
        /// Postal/zip code
        /// </summary>
        public string PostalCode { get => GetValue<string>(); internal set => SetValue(value); }

        /// <summary>
        /// Latitude of the location
        /// </summary>
        public double Latitude { get => GetValue<double>(); internal set => SetValue(value); }

        /// <summary>
        /// Longitude of the location
        /// </summary>
        public double Longitude { get => GetValue<double>(); internal set => SetValue(value); }

        internal override IFieldValue FromJson(JsonElement json)
        {

            if (json.ValueKind == JsonValueKind.Null || json.ValueKind == JsonValueKind.Undefined)
            {
                DisplayName = null;
                LocationUri = null;
                Street = null;
                City = null;
                State = null;
                CountryOrRegion = null;
                PostalCode = null;
                Latitude = 0;
                Longitude = 0;
            }
            else
            {
#pragma warning disable CA1507 // Use nameof to express symbol names
                if (json.TryGetProperty("DisplayName", out JsonElement displayName) || json.TryGetProperty("displayName", out displayName))
#pragma warning restore CA1507 // Use nameof to express symbol names
                {
                    DisplayName = displayName.GetString();
                }

#pragma warning disable CA1507 // Use nameof to express symbol names
                if (json.TryGetProperty("LocationUri", out JsonElement locationUri) || json.TryGetProperty("locationUri", out locationUri))
#pragma warning restore CA1507 // Use nameof to express symbol names
                {
                    LocationUri = locationUri.GetString();
                }

                if (json.TryGetProperty("Address", out JsonElement address) || json.TryGetProperty("address", out address))
                {
#pragma warning disable CA1507 // Use nameof to express symbol names
                    if (address.TryGetProperty("Street", out JsonElement street) || address.TryGetProperty("street", out street))
#pragma warning restore CA1507 // Use nameof to express symbol names
                    {
                        Street = street.GetString();
                    }

#pragma warning disable CA1507 // Use nameof to express symbol names
                    if (address.TryGetProperty("City", out JsonElement city) || address.TryGetProperty("city", out city))
#pragma warning restore CA1507 // Use nameof to express symbol names
                    {
                        City = city.GetString();
                    }

#pragma warning disable CA1507 // Use nameof to express symbol names
                    if (address.TryGetProperty("State", out JsonElement state) || address.TryGetProperty("state", out  state))
#pragma warning restore CA1507 // Use nameof to express symbol names
                    {
                        State = state.GetString();
                    }

#pragma warning disable CA1507 // Use nameof to express symbol names
                    if (address.TryGetProperty("CountryOrRegion", out JsonElement countryOrRegion) || address.TryGetProperty("countryOrRegion", out countryOrRegion))
#pragma warning restore CA1507 // Use nameof to express symbol names
                    {
                        CountryOrRegion = countryOrRegion.GetString();
                    }

#pragma warning disable CA1507 // Use nameof to express symbol names
                    if (address.TryGetProperty("PostalCode", out JsonElement postalCode) || address.TryGetProperty("postalCode", out postalCode))
#pragma warning restore CA1507 // Use nameof to express symbol names
                    {
                        PostalCode = postalCode.GetString();
                    }
                }

                if (json.TryGetProperty("Coordinates", out JsonElement coordinates) || json.TryGetProperty("coordinates", out coordinates))
                {
#pragma warning disable CA1507 // Use nameof to express symbol names
                    if (coordinates.TryGetProperty("Latitude", out JsonElement latitude) || coordinates.TryGetProperty("latitude", out latitude))
#pragma warning restore CA1507 // Use nameof to express symbol names
                    {
                        if (latitude.ValueKind == JsonValueKind.Number)
                        {
                            Latitude = latitude.GetDouble();
                        }
                    }

#pragma warning disable CA1507 // Use nameof to express symbol names
                    if (coordinates.TryGetProperty("Longitude", out JsonElement longitude) || coordinates.TryGetProperty("longitude", out longitude))
#pragma warning restore CA1507 // Use nameof to express symbol names
                    {
                        if (longitude.ValueKind == JsonValueKind.Number)
                        {
                            Longitude = longitude.GetDouble();
                        }
                    }
                }
            }

            // Clear changes
            Commit();

            return this;
        }

        internal override IFieldValue FromListDataAsStream(Dictionary<string, string> properties)
        {
            if (!properties.ContainsKey("Latitude") || !properties.ContainsKey("DisplayName"))
            {
                DisplayName = null;
                LocationUri = null;
                Street = null;
                City = null;
                State = null;
                CountryOrRegion = null;
                PostalCode = null;
                Latitude = 0;
                Longitude = 0;
            }
            else
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
            }

            // Clear changes
            Commit();

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
            var addFieldByCoordinates = new
            {
                EntityType = "Custom",
                DisplayName,
                Coordinates = new
                {
                    Latitude,
                    Longitude
                }
            };

            var bodyContent = JsonSerializer.Serialize(addFieldByCoordinates, PnPConstants.JsonSerializer_IgnoreNullValues_CamelCase);
            return bodyContent;
        }

        internal override string ToCsomXml()
        {
            //throw new NotImplementedException();
            return "";
        }

    }
}
