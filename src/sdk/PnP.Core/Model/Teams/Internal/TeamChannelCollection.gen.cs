namespace PnP.Core.Model.Teams
{
    internal partial class TeamChannelCollection : BaseDataModelCollection<ITeamChannel>, ITeamChannelCollection
    {
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
