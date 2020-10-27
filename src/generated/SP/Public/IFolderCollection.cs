using PnP.Core.Services;
using System.Linq;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// Public interface to define a collection of Folder objects
    /// </summary>
    [ConcreteType(typeof(FolderCollection))]
    public interface IFolderCollection : IQueryable<IFolder>, IDataModelCollection<IFolder>
    {
    }
}