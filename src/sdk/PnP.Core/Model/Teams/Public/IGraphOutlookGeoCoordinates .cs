namespace PnP.Core.Model.Teams
{
    /// <summary>
    /// The geographic coordinates, elevation, and their degree of accuracy for a physical location.
    /// </summary>
    [ConcreteType(typeof(GraphOutlookGeoCoordinates))]
    public interface IGraphOutlookGeoCoordinates : IDataModel<IGraphOutlookGeoCoordinates>
    {
        /// <summary>
        /// The accuracy of the latitude and longitude. As an example, the accuracy can be measured in meters, such as the latitude and longitude are accurate to within 50 meters.
        /// </summary>
        public double Accuracy { get; }

        /// <summary>
        /// The altitude of the location.
        /// </summary>
        public double Altitude { get; }

        /// <summary>
        /// The accuracy of the altitude.
        /// </summary>
        public double AltitudeAccuracy { get; set; }

        /// <summary>
        /// The latitude of the location.
        /// </summary>
        public double Latitude { get; set; }

        /// <summary>
        /// The longitude of the location.
        /// </summary>
        public double Longitude { get; set; }
    }
}
