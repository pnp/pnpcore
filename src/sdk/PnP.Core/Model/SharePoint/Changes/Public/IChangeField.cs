using System;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// Indicates a change to a <seealso cref="IField"/> object.
    /// </summary>
    /// <seealso cref="IChange" />
    [ConcreteType(typeof(ChangeField))]
    public interface IChangeField : IChange
    {
        /// <summary>
        /// Gets a value that identifies the changed field.
        /// </summary>
        public Guid FieldId { get; }

        /// <summary>
        /// Gets a value that identifies the web that contains the changed field.
        /// </summary>
        public Guid WebId { get; }
    }
}