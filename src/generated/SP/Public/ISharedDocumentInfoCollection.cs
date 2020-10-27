using PnP.Core.Services;
using System.Linq;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// Public interface to define a collection of SharedDocumentInfo objects
    /// </summary>
    [ConcreteType(typeof(SharedDocumentInfoCollection))]
    public interface ISharedDocumentInfoCollection : IQueryable<ISharedDocumentInfo>, IDataModelCollection<ISharedDocumentInfo>
    {
    }
}