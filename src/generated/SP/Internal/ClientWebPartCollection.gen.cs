using PnP.Core.QueryModel.Model;
using PnP.Core.Services;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// Collection of ClientWebPart Domain Model objects
    /// </summary>
    internal partial class ClientWebPartCollection : QueryableDataModelCollection<IClientWebPart>, IClientWebPartCollection
    {
        public ClientWebPartCollection(PnPContext context, IDataModelParent parent) : base(context, parent)
        {
            this.PnPContext = context;
            this.Parent = parent;
        }
    }
}