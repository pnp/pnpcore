using System.Linq;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// Public interface to define a collection of ContentType objects
    /// </summary>
    public interface IContentTypeCollection : IQueryable<IContentType>, IDataModelCollection<IContentType>
    {
    }
}