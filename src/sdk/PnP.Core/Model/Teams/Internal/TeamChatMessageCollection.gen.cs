namespace PnP.Core.Model.Teams
{
    internal partial class TeamChatMessageCollection: BaseDataModelCollection<ITeamChatMessage>, ITeamChatMessageCollection
    {
        public override ITeamChatMessage CreateNew()
        {
            return NewTeamChatMessage();
        }

        private TeamChatMessage AddNewTeamChatMessage()
        {
            var newTeamChatMessage = NewTeamChatMessage();
            this.items.Add(newTeamChatMessage);
            return newTeamChatMessage;
        }

        private TeamChatMessage NewTeamChatMessage()
        {
            var newTeamChatMessage = new TeamChatMessage
            {
                PnPContext = this.PnPContext,
                Parent = this,
            };
            return newTeamChatMessage;
        }
    }
}
