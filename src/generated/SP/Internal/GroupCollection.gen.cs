using PnP.Core.QueryModel.Model;
using PnP.Core.Services;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// Collection of Group Domain Model objects
    /// </summary>
    internal partial class GroupCollection : QueryableDataModelCollection<IGroup>, IGroupCollection
    {
        public GroupCollection(PnPContext context, IDataModelParent parent) : base(context, parent)
        {
            this.PnPContext = context;
            this.Parent = parent;
        }
    }
}