using PnP.Core.Model.Security;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// Indicates a change to a <seealso cref="ISharePointGroup"/> object.
    /// </summary>
    /// <seealso cref="PnP.Core.Model.SharePoint.IChange" />
    [ConcreteType(typeof(ChangeGroup))]
    public interface IChangeGroup : IChange
    {
        /// <summary>
        /// Gets a value that identifies the changed group.
        /// </summary>
        public int GroupId { get; }
    }
}