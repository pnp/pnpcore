using System;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// Indicates a change to a <seealso cref="IWeb"/> object.
    /// </summary>
    /// <seealso cref="IChange" />
    [ConcreteType(typeof(ChangeWeb))]
    public interface IChangeWeb : IChange
    {
        /// <summary>
        /// Gets a value that identifies the changed web.
        /// </summary>
        public Guid WebId { get; }
    }
}