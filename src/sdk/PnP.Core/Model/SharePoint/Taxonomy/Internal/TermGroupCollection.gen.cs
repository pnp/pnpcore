using PnP.Core.QueryModel.Model;
using PnP.Core.Services;

namespace PnP.Core.Model.SharePoint
{
    internal partial class TermGroupCollection : QueryableDataModelCollection<ITermGroup>, ITermGroupCollection
    {
        public TermGroupCollection(PnPContext context, IDataModelParent parent, string memberName = null)
            : base(context, parent, memberName)
        {
            this.PnPContext = context;
            this.Parent = parent;
        }
    }
}
