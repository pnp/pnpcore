using System;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// Base interface for a description of a change to an object.
    /// </summary>
    [ConcreteType(typeof(Change))]
    public interface IChange
    {
        /// <summary>
        /// Gets a value that specifies a change token that identifies the changed object.
        /// </summary>
        public IChangeToken ChangeToken { get; }

        /// <summary>
        /// Gets a value that specifies the type of the change that has been made to the object.
        /// </summary>
        public ChangeType ChangeType { get; }

        /// <summary>
        /// A string representing the relative value of Time.
        /// </summary>
        /// <returns>
        /// The return value is in the form mode|args
        /// mode=0: Special case where args is passed through as the display text
        /// mode=1: Standard friendly relative display mode, eg "In 3 hours"
        ///         Format: mode|bFuture|bucket|args
        ///         bFuture refers to if Time is at a later time than UtcNow
        ///         bucket is the type of string to output
        ///         args contains the values to plug into the string referenced by the bucket
        /// </returns>
        public string RelativeTime { get; }

        /// <summary>
        /// Gets a value that identifies the site that contains the changed object.
        /// </summary>
        public Guid SiteId { get; }

        /// <summary>
        /// Gets a value that specifies the time that the object was modified.
        /// </summary>
        public DateTime Time { get; }
    }
}
