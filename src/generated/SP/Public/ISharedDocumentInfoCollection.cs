using System.Linq;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// Public interface to define a collection of SharedDocumentInfo objects
    /// </summary>
    public interface ISharedDocumentInfoCollection : IQueryable<ISharedDocumentInfo>, IDataModelCollection<ISharedDocumentInfo>
    {
    }
}