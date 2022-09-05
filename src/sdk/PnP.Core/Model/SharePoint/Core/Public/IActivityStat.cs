using System;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// Defines the site/item analytics for a given interval
    /// </summary>
    public interface IActivityStat
    {
        /// <summary>
        /// Start date for the interval (UTC)
        /// </summary>
        DateTime StartDateTime { get; }

        /// <summary>
        /// End date for the interval (UTC)
        /// </summary>
        DateTime EndDateTime { get; }

        /// <summary>
        /// Indicates whether the item is "trending."
        /// </summary>
        bool IsTrending { get; }

        /// <summary>
        /// Statistics about the create actions in this interval.
        /// </summary>
        IActivityActionStat Create { get; }

        /// <summary>
        /// Statistics about the delete actions in this interval.
        /// </summary>
        IActivityActionStat Delete { get; }

        /// <summary>
        /// Statistics about the edit actions in this interval.
        /// </summary>
        IActivityActionStat Edit { get; }

        /// <summary>
        /// Statistics about the move actions in this interval.
        /// </summary>
        IActivityActionStat Move { get; }

        /// <summary>
        /// Statistics about the access actions in this interval.
        /// </summary>
        IActivityActionStat Access { get; }

        /// <summary>
        /// Indicates that the statistics in this interval are based on incomplete data.
        /// </summary>
        IActivityIncomplete IncompleteData { get; }
    }
}
