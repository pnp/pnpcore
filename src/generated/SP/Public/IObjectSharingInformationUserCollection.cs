using PnP.Core.Services;
using System.Linq;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// Public interface to define a collection of ObjectSharingInformationUser objects
    /// </summary>
    [ConcreteType(typeof(ObjectSharingInformationUserCollection))]
    public interface IObjectSharingInformationUserCollection : IQueryable<IObjectSharingInformationUser>, IDataModelCollection<IObjectSharingInformationUser>
    {
    }
}