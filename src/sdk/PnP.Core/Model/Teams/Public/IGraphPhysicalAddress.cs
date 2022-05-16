namespace PnP.Core.Model.Teams
{
    /// <summary>
    /// Represents the street address of a resource such as a contact or event.
    /// </summary>
    [ConcreteType(typeof(GraphPhysicalAddress))]
    public interface IGraphPhysicalAddress : IDataModel<IGraphPhysicalAddress>
    {
        /// <summary>
        /// The city.
        /// </summary>
        public string City { get; set; }

        /// <summary>
        /// The country or region. It's a free-format string value, for example, "United States".
        /// </summary>
        public string CountryOrRegion { get; set; }

        /// <summary>
        /// The postal code.
        /// </summary>
        public string PostalCode { get; set; }

        /// <summary>
        /// The state.
        /// </summary>
        public string State { get; set; }

        /// <summary>
        /// The street.
        /// </summary>
        public string Street { get; set; }
    }
}
