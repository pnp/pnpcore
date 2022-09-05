using System;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// Indicates that a resource was generated with incomplete data. The properties within might provide information about why the data is incomplete.
    /// </summary>
    public interface IActivityIncomplete
    {
        /// <summary>
        /// The service does not have source data before the specified time.
        /// </summary>
        DateTime MissingDataBeforeDateTime { get; }

        /// <summary>
        /// Some data was not recorded due to excessive activity.
        /// </summary>
        bool WasThrottled { get; }

        /// <summary>
        /// Some data is still pending processing.
        /// </summary>
        bool ResultsPending { get; }

        /// <summary>
        /// Not supported
        /// </summary>
        bool NotSupported { get; }
    }
}
