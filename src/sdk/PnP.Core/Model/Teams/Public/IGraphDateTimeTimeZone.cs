namespace PnP.Core.Model.Teams
{
    /// <summary>
    /// Describes the date, time, and time zone of a point in time.
    /// </summary>
    [ConcreteType(typeof(GraphDateTimeTimeZone))]
    public interface IGraphDateTimeTimeZone : IDataModel<IGraphDateTimeTimeZone>
    {
        /// <summary>
        /// A single point of time in a combined date and time representation ({date}T{time}; for example, 2017-08-29T04:00:00.0000000).
        /// </summary>
        public string DateTime { get; set; }

        /// <summary>
        /// Represents a time zone, for example, "Pacific Standard Time".
        /// </summary>
        public string TimeZone { get; }
    }
}
