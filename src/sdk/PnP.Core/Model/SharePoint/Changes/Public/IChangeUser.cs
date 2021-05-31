using PnP.Core.Model.Security;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// Indicates a change to a <seealso cref="ISharePointUser"/> object.
    /// </summary>
    /// <seealso cref="PnP.Core.Model.SharePoint.IChange" />
    [ConcreteType(typeof(ChangeUser))]
    public interface IChangeUser : IChange
    {
        /// <summary>
        /// Gets a value that specifies whether a user has changed from an inactive state to an active state.
        /// </summary>
        public bool Activate { get; }

        /// <summary>
        /// Gets a value that identifies the changed user.
        /// </summary>
        public int UserId { get; }
    }
}