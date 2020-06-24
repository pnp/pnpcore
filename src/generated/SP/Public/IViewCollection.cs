using System.Linq;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// Public interface to define a collection of View objects
    /// </summary>
    public interface IViewCollection : IQueryable<IView>, IDataModelCollection<IView>
    {
    }
}