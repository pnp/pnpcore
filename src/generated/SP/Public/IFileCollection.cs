using PnP.Core.Services;
using System.Linq;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// Public interface to define a collection of File objects
    /// </summary>
    [ConcreteType(typeof(FileCollection))]
    public interface IFileCollection : IQueryable<IFile>, IDataModelCollection<IFile>
    {
    }
}