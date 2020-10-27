using PnP.Core.Services;
using System.Linq;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// Public interface to define a collection of ContentType objects
    /// </summary>
    [ConcreteType(typeof(ContentTypeCollection))]
    public interface IContentTypeCollection : IQueryable<IContentType>, IDataModelCollection<IContentType>
    {
    }
}