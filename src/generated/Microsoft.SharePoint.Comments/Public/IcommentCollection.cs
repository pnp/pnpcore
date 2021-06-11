using PnP.Core.Services;
using System.Linq;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// Public interface to define a collection of comment objects
    /// </summary>
    [ConcreteType(typeof(commentCollection))]
    public interface IcommentCollection : IQueryable<Icomment>, IDataModelCollection<Icomment>
    {
    }
}