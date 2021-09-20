using System;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// Indicates a change to a <seealso cref="IContentType"/> object.
    /// </summary>
    /// <seealso cref="IChange" />
    [ConcreteType(typeof(ChangeContentType))]
    public interface IChangeContentType : IChange
    {
        /// <summary>
        /// Gets a value that identifies the changed content type.
        /// </summary>
        public IContentType ContentTypeId { get; }

        /// <summary>
        /// Gets a value that identifies the web that contains the changed content type.
        /// </summary>
        public Guid WebId { get; }
    }
}