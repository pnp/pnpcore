using System.Linq;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// Public interface to define a collection of NavigationNode objects
    /// </summary>
    public interface INavigationNodeCollection : IQueryable<INavigationNode>, IDataModelCollection<INavigationNode>
    {
    }
}