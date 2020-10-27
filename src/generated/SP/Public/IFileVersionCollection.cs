using PnP.Core.Services;
using System.Linq;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// Public interface to define a collection of FileVersion objects
    /// </summary>
    [ConcreteType(typeof(FileVersionCollection))]
    public interface IFileVersionCollection : IQueryable<IFileVersion>, IDataModelCollection<IFileVersion>
    {
    }
}