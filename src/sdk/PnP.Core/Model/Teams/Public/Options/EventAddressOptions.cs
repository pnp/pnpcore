namespace PnP.Core.Model.Teams
{
    /// <summary>
    /// Options that can be set when creating an address location for a meeting request
    /// </summary>
    public class EventAddressOptions
    {

        /// <summary>
        /// Street
        /// </summary>
        public string Street { get; set; }

        /// <summary>
        /// City
        /// </summary>
        public string City { get; set; }

        /// <summary>
        /// State
        /// </summary>
        public string State { get; set; }

        /// <summary>
        /// Country Or Region
        /// </summary>
        public string CountryOrRegion { get; set; }

        /// <summary>
        /// PostalCode
        /// </summary>
        public string PostalCode { get; set; }

    }
}
