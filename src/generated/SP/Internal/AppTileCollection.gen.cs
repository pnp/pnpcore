using PnP.Core.QueryModel.Model;
using PnP.Core.Services;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// Collection of AppTile Domain Model objects
    /// </summary>
    internal partial class AppTileCollection : QueryableDataModelCollection<IAppTile>, IAppTileCollection
    {
        public AppTileCollection(PnPContext context, IDataModelParent parent) : base(context, parent)
        {
            this.PnPContext = context;
            this.Parent = parent;
        }
    }
}