using System;
using System.Collections.Generic;

namespace PnP.Core.Model.Teams
{
    /// <summary>
    /// Options that can be set when creating a coordinate location for a meeting request
    /// </summary>
    public class EventCoordinateOptions
    {

        /// <summary>
        /// Latitude
        /// </summary>
        public double Latitude { get; set; }

        /// <summary>
        /// Longitude
        /// </summary>
        public double Longitude { get; set; }

        /// <summary>
        /// Accuracy
        /// </summary>
        public double Accuracy { get; set; }

        /// <summary>
        /// Altitude
        /// </summary>
        public double Altitude { get; set; }

        /// <summary>
        /// Altitude Accuracy
        /// </summary>
        public double AltitudeAccuracy { get; set; }
    }
}
