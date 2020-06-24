using PnP.Core.QueryModel.Model;
using PnP.Core.Services;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// Collection of SharedDocumentInfo Domain Model objects
    /// </summary>
    internal partial class SharedDocumentInfoCollection : QueryableDataModelCollection<ISharedDocumentInfo>, ISharedDocumentInfoCollection
    {
        public SharedDocumentInfoCollection(PnPContext context, IDataModelParent parent) : base(context, parent)
        {
            this.PnPContext = context;
            this.Parent = parent;
        }
    }
}