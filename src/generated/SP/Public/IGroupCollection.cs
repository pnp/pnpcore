using System.Linq;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// Public interface to define a collection of Group objects
    /// </summary>
    public interface IGroupCollection : IQueryable<IGroup>, IDataModelCollection<IGroup>
    {
    }
}