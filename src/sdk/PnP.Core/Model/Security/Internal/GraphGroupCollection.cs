using PnP.Core.QueryModel;
using PnP.Core.Services;

namespace PnP.Core.Model.Security
{
    internal sealed class GraphGroupCollection : QueryableDataModelCollection<IGraphGroup>, IGraphGroupCollection
    {
        public GraphGroupCollection(PnPContext context, IDataModelParent parent, string memberName = null) : base(context, parent, memberName)
        {
            PnPContext = context;
            Parent = parent;
        }
    }
}
