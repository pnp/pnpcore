using System;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// Indicates a change to an Alert object.
    /// </summary>
    /// <seealso cref="IChange" />
    [ConcreteType(typeof(ChangeAlert))]
    public interface IChangeAlert : IChange
    {
        /// <summary>
        /// Gets a value that identifies the changed alert.
        /// </summary>
        public Guid AlertId { get; }

        /// <summary>
        /// Gets a value that identifies the web that contains the changed alert.
        /// </summary>
        public Guid WebId { get; }
    }
}