using PnP.Core.QueryModel.Model;
using PnP.Core.Services;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// Collection of UserCustomAction Domain Model objects
    /// </summary>
    internal partial class UserCustomActionCollection : QueryableDataModelCollection<IUserCustomAction>, IUserCustomActionCollection
    {
        public UserCustomActionCollection(PnPContext context, IDataModelParent parent) : base(context, parent)
        {
            this.PnPContext = context;
            this.Parent = parent;
        }
    }
}