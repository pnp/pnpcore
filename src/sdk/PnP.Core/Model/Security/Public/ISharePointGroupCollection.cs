using System.Linq;

namespace PnP.Core.Model.Security
{
    /// <summary>
    /// Public interface to define a collection of SharePoint groups
    /// </summary>
    public interface ISharePointGroupCollection : IQueryable<ISharePointGroup>, IDataModelCollection<ISharePointGroup>
    {
    }
}
