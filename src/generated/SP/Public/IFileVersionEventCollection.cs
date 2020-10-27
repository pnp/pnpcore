using PnP.Core.Services;
using System.Linq;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// Public interface to define a collection of FileVersionEvent objects
    /// </summary>
    [ConcreteType(typeof(FileVersionEventCollection))]
    public interface IFileVersionEventCollection : IQueryable<IFileVersionEvent>, IDataModelCollection<IFileVersionEvent>
    {
    }
}