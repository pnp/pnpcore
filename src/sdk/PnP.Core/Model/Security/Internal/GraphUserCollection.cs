using PnP.Core.QueryModel;
using PnP.Core.Services;

namespace PnP.Core.Model.Security
{
    internal partial class GraphUserCollection : QueryableDataModelCollection<IGraphUser>, IGraphUserCollection
    {
        public GraphUserCollection(PnPContext context, IDataModelParent parent, string memberName = null) : base(context, parent, memberName)
        {
            PnPContext = context;
            Parent = parent;
        }
    }
}
