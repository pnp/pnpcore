using PnP.Core.QueryModel;
using PnP.Core.Services;

namespace PnP.Core.Model.Security
{
    internal sealed class GraphPermissionCollection : BaseDataModelCollection<IGraphPermission>, IGraphPermissionCollection
    {
        public GraphPermissionCollection(PnPContext context, IDataModelParent parent)
        {
            Parent = parent;
            PnPContext = context;
        }
    }
}
