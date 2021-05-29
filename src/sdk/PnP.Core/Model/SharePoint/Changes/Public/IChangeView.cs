using System;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// Indicates a change to a <seealso cref="IView"/> object.
    /// </summary>
    /// <seealso cref="PnP.Core.Model.SharePoint.IChange" />
    [ConcreteType(typeof(ChangeView))]
    public interface IChangeView : IChange
    {
        /// <summary>
        /// Gets a value that identifies the changed view.
        /// </summary>
        public Guid ViewId { get; }

        /// <summary>
        /// Gets a value that identifies the list that contains the changed view.
        /// </summary>
        public Guid ListId { get; }

        /// <summary>
        /// Gets a value that identifies the web that contains the changed view.
        /// </summary>
        public Guid WebId { get; }
    }
}