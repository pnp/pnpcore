using System.Linq;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// Public interface to define a collection of Folder objects
    /// </summary>
    public interface IFolderCollection : IQueryable<IFolder>, IDataModelCollection<IFolder>
    {
    }
}