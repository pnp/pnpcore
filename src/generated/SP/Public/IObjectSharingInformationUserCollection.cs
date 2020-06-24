using System.Linq;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// Public interface to define a collection of ObjectSharingInformationUser objects
    /// </summary>
    public interface IObjectSharingInformationUserCollection : IQueryable<IObjectSharingInformationUser>, IDataModelCollection<IObjectSharingInformationUser>
    {
    }
}