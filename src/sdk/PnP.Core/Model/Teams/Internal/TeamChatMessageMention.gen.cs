namespace PnP.Core.Model.Teams
{
    [GraphType]
    internal partial class TeamChatMessageMention : BaseDataModel<ITeamChatMessageMention>, ITeamChatMessageMention
    {
        public int Id { get => GetValue<int>(); set => SetValue(value); }

        public string MentionText { get => GetValue<string>(); set => SetValue(value); }

        //public ITeamIdentitySet Mentioned { get => GetValue<ITeamIdentitySet>(); set => SetValue(value); }
        public ITeamIdentitySet Mentioned
        {
            get
            {
                if (!NavigationPropertyInstantiated())
                {
                    var teamIdentitySet = new TeamIdentitySet
                    {
                        PnPContext = this.PnPContext,
                        Parent = this,
                    };
                    SetValue(teamIdentitySet);
                    InstantiateNavigationProperty();
                }
                return GetValue<ITeamIdentitySet>();
            }
            set
            {
                InstantiateNavigationProperty();
                SetValue(value);
            }
        }
    }
}
