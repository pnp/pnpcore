using System.Linq;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// Public interface to define a collection of Alert objects
    /// </summary>
    public interface IAlertCollection : IQueryable<IAlert>, IDataModelCollection<IAlert>
    {
    }
}