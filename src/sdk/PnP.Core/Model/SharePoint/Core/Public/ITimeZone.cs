namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// Public interface to define a TimeZone object
    /// </summary>
    [ConcreteType(typeof(TimeZone))]
    public interface ITimeZone : IDataModel<ITimeZone>, IDataModelGet<ITimeZone>
    {
        #region Properties

        /// <summary>
        /// Time zone description
        /// </summary>
        public string Description { get; }

        /// <summary>
        /// Id of the time zone
        /// </summary>
        public int Id { get; }

        /// <summary>
        /// Bias (additional minutes to get to UTC time) for this time zone
        /// </summary>
        public int Bias { get; }

        /// <summary>
        /// Bias (additional minutes to get to UTC time) for this time zone when in daylight saving
        /// </summary>
        public int DaylightBias { get; }

        /// <summary>
        /// Bias (additional minutes to get to UTC time) for this time zone when not in daylight saving
        /// </summary>
        public int StandardBias { get; }
        #endregion

    }
}
