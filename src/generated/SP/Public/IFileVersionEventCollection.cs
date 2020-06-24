using System.Linq;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// Public interface to define a collection of FileVersionEvent objects
    /// </summary>
    public interface IFileVersionEventCollection : IQueryable<IFileVersionEvent>, IDataModelCollection<IFileVersionEvent>
    {
    }
}