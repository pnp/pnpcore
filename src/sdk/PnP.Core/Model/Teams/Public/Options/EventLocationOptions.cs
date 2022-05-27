namespace PnP.Core.Model.Teams
{
    /// <summary>
    /// Options that can be set when creating a location for a meeting request
    /// </summary>
    public class EventLocationOptions
    {

        /// <summary>
        /// Type of location
        /// </summary>
        public EventLocationType Type { get; set; }

        /// <summary>
        /// Name of the location
        /// </summary>
        public string DisplayName { get; set; }

        /// <summary>
        /// Location email address
        /// </summary>
        public string LocationEmailAddress { get; set; }

        /// <summary>
        /// Location Uri
        /// </summary>
        public string LocationUri { get; set; }

        /// <summary>
        /// Address options
        /// </summary>
        public EventAddressOptions Address { get; set; }

        /// <summary>
        /// Coordinates options
        /// </summary>
        public EventCoordinateOptions Coordinates { get; set; }
    }
}
