using PnP.Core.Services;
using System.Linq;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// Public interface to define a collection of SharingPermissionInformation objects
    /// </summary>
    [ConcreteType(typeof(SharingPermissionInformationCollection))]
    public interface ISharingPermissionInformationCollection : IQueryable<ISharingPermissionInformation>, IDataModelCollection<ISharingPermissionInformation>
    {
    }
}