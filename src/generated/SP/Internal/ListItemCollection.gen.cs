using PnP.Core.QueryModel.Model;
using PnP.Core.Services;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// Collection of ListItem Domain Model objects
    /// </summary>
    internal partial class ListItemCollection : QueryableDataModelCollection<IListItem>, IListItemCollection
    {
        public ListItemCollection(PnPContext context, IDataModelParent parent) : base(context, parent)
        {
            this.PnPContext = context;
            this.Parent = parent;
        }
    }
}