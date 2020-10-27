using PnP.Core.Services;
using System.Linq;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// Public interface to define a collection of View objects
    /// </summary>
    [ConcreteType(typeof(ViewCollection))]
    public interface IViewCollection : IQueryable<IView>, IDataModelCollection<IView>
    {
    }
}