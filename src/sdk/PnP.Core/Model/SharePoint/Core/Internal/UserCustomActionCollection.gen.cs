using PnP.Core.QueryModel;
using PnP.Core.Services;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// Collection of UserCustomAction Domain Model objects
    /// </summary>
    internal partial class UserCustomActionCollection : QueryableDataModelCollection<IUserCustomAction>, IUserCustomActionCollection
    {
        public UserCustomActionCollection(PnPContext context, IDataModelParent parent, string memberName = null) : base(context, parent, memberName)
        {
            this.PnPContext = context;
            this.Parent = parent;
        }
    }
}