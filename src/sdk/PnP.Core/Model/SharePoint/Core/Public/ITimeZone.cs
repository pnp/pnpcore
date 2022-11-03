using System;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// Public interface to define a TimeZone object
    /// </summary>
    [ConcreteType(typeof(TimeZone))]
    public interface ITimeZone : IDataModel<ITimeZone>, IDataModelGet<ITimeZone>, IDataModelLoad<ITimeZone>, IQueryableDataModel
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

        /// <summary>
        /// A special property used to add an asterisk to a $select statement
        /// </summary>
        public object All { get; }
        #endregion

        #region Methods

        /// <summary>
        /// Converts the specified DateTime value from local time to Coordinated Universal Time (UTC).
        /// </summary>
        /// <param name="dateTime">A <see cref="DateTime"/> object that represents the local date and time value to convert.</param>
        /// <returns>A <see cref="DateTime"/> object that contains the date converted to UTC.</returns>
        public DateTime LocalTimeToUtc(DateTime dateTime);

        /// <summary>
        /// Converts the specified DateTime value from Coordinated Universal Time (UTC) to local time.
        /// </summary>
        /// <param name="dateTime">A <see cref="DateTime"/> object that represents the UTC date and time value to convert.</param>
        /// <returns>A <see cref="DateTime"/> structure that contains the date and time converted to their local values.</returns>
        public DateTime UtcToLocalTime(DateTime dateTime);

        /// <summary>
        /// Returns the current SharePoint timezone information as a Windows TimeZoneInfo object
        /// </summary>
        /// <returns></returns>
        public TimeZoneInfo GetTimeZoneInfo();
        #endregion

    }
}
