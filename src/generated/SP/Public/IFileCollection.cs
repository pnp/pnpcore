using System.Linq;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// Public interface to define a collection of File objects
    /// </summary>
    public interface IFileCollection : IQueryable<IFile>, IDataModelCollection<IFile>
    {
    }
}