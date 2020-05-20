using PnP.Core.QueryModel.Model;
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

        public override ITeamChannel CreateNew()
        {
            return NewTeamChannel();
        }

        private TeamChannel AddNewTeamChannel()
        {
            var newTeamChannel = NewTeamChannel();
            this.items.Add(newTeamChannel);
            return newTeamChannel;
        }

        private TeamChannel NewTeamChannel()
        {
            var newTeamChannel = new TeamChannel
            {
                PnPContext = this.PnPContext,
                Parent = this,
            };
            return newTeamChannel;
        }
    }
}
