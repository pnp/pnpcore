using System;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// Indicates a change to a <seealso cref="IFile"/> object.
    /// </summary>
    /// <seealso cref="PnP.Core.Model.SharePoint.IChange" />
    [ConcreteType(typeof(ChangeFile))]
    public interface IChangeFile : IChange
    {
        /// <summary>
        /// Gets a value that identifies the changed file.
        /// </summary>
        public Guid UniqueId { get; }

        /// <summary>
        /// Gets a value that identifies the web that contains the changed file.
        /// </summary>
        public Guid WebId { get; }
    }
}