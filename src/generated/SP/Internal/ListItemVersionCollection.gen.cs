using PnP.Core.QueryModel.Model;
using PnP.Core.Services;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// Collection of ListItemVersion Domain Model objects
    /// </summary>
    internal partial class ListItemVersionCollection : QueryableDataModelCollection<IListItemVersion>, IListItemVersionCollection
    {
        public ListItemVersionCollection(PnPContext context, IDataModelParent parent) : base(context, parent)
        {
            this.PnPContext = context;
            this.Parent = parent;
        }
    }
}