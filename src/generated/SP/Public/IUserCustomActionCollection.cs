using System.Linq;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// Public interface to define a collection of UserCustomAction objects
    /// </summary>
    public interface IUserCustomActionCollection : IQueryable<IUserCustomAction>, IDataModelCollection<IUserCustomAction>
    {
    }
}