using PnP.Core.Services;
using System.Linq;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// Public interface to define a collection of Web objects
    /// </summary>
    [ConcreteType(typeof(WebCollection))]
    public interface IWebCollection : IQueryable<IWeb>, IDataModelCollection<IWeb>
    {
    }
}