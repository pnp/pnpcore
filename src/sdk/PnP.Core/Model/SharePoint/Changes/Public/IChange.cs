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
