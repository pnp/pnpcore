using PnP.Core.QueryModel.Model;
using PnP.Core.Services;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// Collection of User Domain Model objects
    /// </summary>
    internal partial class UserCollection : QueryableDataModelCollection<IUser>, IUserCollection
    {
        public UserCollection(PnPContext context, IDataModelParent parent) : base(context, parent)
        {
            this.PnPContext = context;
            this.Parent = parent;
        }
    }
}