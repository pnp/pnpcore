namespace PnP.Core.Model.Teams
{
    /// <summary>
    /// Represents location information of an event.
    /// </summary>
    [ConcreteType(typeof(GraphLocation))]
    public interface IGraphLocation : IDataModel<IGraphLocation>
    {
        /// <summary>
        /// The street address of the location.
        /// </summary>
        public IGraphPhysicalAddress Address { get; set; }

        /// <summary>
        /// The geographic coordinates and elevation of the location.
        /// </summary>
        public IGraphOutlookGeoCoordinates Coordinates { get; set; }

        /// <summary>
        /// The name associated with the location.
        /// </summary>
        public string DisplayName { get; }

        /// <summary>
        /// Optional email address of the location.
        /// </summary>
        public string LocationEmailAddress { get; }

        /// <summary>
        /// Optional URI representing the location.
        /// </summary>
        public string LocationUri { get; }

        /// <summary>
        /// The type of location
        /// </summary>
        public EventLocationType LocationType { get; }

        /// <summary>
        /// For internal use only.
        /// </summary>
        public string UniqueId { get; }

        /// <summary>
        /// For internal use only.
        /// </summary>
        public string UniqueIdType { get; }
    }
}
