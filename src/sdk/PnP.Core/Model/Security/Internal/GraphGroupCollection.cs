using PnP.Core.QueryModel;
using PnP.Core.Services;

namespace PnP.Core.Model.Security
{
    internal partial class GraphGroupCollection : QueryableDataModelCollection<IGraphGroup>, IGraphGroupCollection
    {
        public GraphGroupCollection(PnPContext context, IDataModelParent parent, string memberName = null) : base(context, parent, memberName)
        {
            this.PnPContext = context;
            this.Parent = parent;
        }
    }
}
