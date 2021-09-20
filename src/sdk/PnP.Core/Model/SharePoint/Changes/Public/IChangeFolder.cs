using System;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// Indicates a change to a <seealso cref="IFolder"/> object.
    /// </summary>
    /// <seealso cref="IChange" />
    [ConcreteType(typeof(ChangeFolder))]
    public interface IChangeFolder : IChange
    {
        /// <summary>
        /// Gets a value that identifies the changed folder.
        /// </summary>
        public Guid UniqueId { get; }

        /// <summary>
        /// Gets a value that identifies the web that contains the changed folder.
        /// </summary>
        public Guid WebId { get; }
    }
}