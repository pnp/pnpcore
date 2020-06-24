using System.Linq;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// Public interface to define a collection of SharingPermissionInformation objects
    /// </summary>
    public interface ISharingPermissionInformationCollection : IQueryable<ISharingPermissionInformation>, IDataModelCollection<ISharingPermissionInformation>
    {
    }
}