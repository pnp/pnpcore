using PnP.Core.QueryModel;
using PnP.Core.Services;
using System.Threading.Tasks;

namespace PnP.Core.Model.Security
{
    internal partial class UserCollection : QueryableDataModelCollection<IUser>, IUserCollection
    {
        public UserCollection(PnPContext context, IDataModelParent parent, string memberName = null) : base(context, parent, memberName)
        {
            this.PnPContext = context;
            this.Parent = parent;
        }
    }
}
