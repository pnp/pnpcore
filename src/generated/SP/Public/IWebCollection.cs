using System.Linq;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// Public interface to define a collection of Web objects
    /// </summary>
    public interface IWebCollection : IQueryable<IWeb>, IDataModelCollection<IWeb>
    {
    }
}