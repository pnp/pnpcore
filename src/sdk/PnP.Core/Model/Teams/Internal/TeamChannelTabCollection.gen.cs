namespace PnP.Core.Model.Teams
{
    internal partial class TeamChannelTabCollection : BaseDataModelCollection<ITeamChannelTab>, ITeamChannelTabCollection
    {
        public override ITeamChannelTab CreateNew()
        {
            return NewTeamChannelTab();
        }

        private TeamChannelTab AddNewTeamChannelTab()
        {
            var newTeamChannelTab = NewTeamChannelTab();
            this.items.Add(newTeamChannelTab);
            return newTeamChannelTab;
        }

        private TeamChannelTab NewTeamChannelTab()
        {
            var newTeamChannelTab = new TeamChannelTab
            {
                PnPContext = this.PnPContext,
                Parent = this,
            };
            return newTeamChannelTab;
        }
    }
}
