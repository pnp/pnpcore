namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// Represents a location field value
    /// </summary>
    public interface IFieldLocationValue : IFieldValue
    {
        /// <summary>
        /// Name identifiying this location
        /// </summary>
        public string DisplayName { get;  }

        /// <summary>
        /// Uri identifying this location
        /// </summary>
        public string LocationUri { get; set; }

        /// <summary>
        /// Streetname
        /// </summary>
        public string Street { get; }

        /// <summary>
        /// City
        /// </summary>
        public string City { get; }

        /// <summary>
        /// State
        /// </summary>
        public string State { get; }

        /// <summary>
        /// Country of region
        /// </summary>
        public string CountryOrRegion { get; }

        /// <summary>
        /// Postal/zip code
        /// </summary>
        public string PostalCode { get; }

        /// <summary>
        /// Latitude of the location
        /// </summary>
        public double Latitude { get; set; }

        /// <summary>
        /// Longitude of the location
        /// </summary>
        public double Longitude { get; set; }
    }
}
