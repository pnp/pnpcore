using PnP.Core.QueryModel;
using PnP.Core.Services;

namespace PnP.Core.Model.Teams
{
    internal sealed class TeamAppCollection : QueryableDataModelCollection<ITeamApp>, ITeamAppCollection
    {
        public TeamAppCollection(PnPContext context, IDataModelParent parent, string memberName = null)
            : base(context, parent, memberName)
        {
            PnPContext = context;
            Parent = parent;
        }
    }
}
