using PnP.Core.QueryModel;
using PnP.Core.Services;

namespace PnP.Core.Model.Teams
{
    internal partial class TeamChannelCollection : QueryableDataModelCollection<ITeamChannel>, ITeamChannelCollection
    {
        public TeamChannelCollection(PnPContext context, IDataModelParent parent, string memberName = null)
            : base(context, parent, memberName)
        {
            this.PnPContext = context;
            this.Parent = parent;
        }

    }
}
