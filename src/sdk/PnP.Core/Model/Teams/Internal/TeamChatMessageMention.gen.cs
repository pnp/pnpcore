namespace PnP.Core.Model.Teams
{
    [GraphType]
    internal partial class TeamChatMessageMention : BaseComplexType<ITeamChatMessageMention>, ITeamChatMessageMention
    {
        public int Id { get => GetValue<int>(); set => SetValue(value); }

        public string MentionText { get => GetValue<string>(); set => SetValue(value); }

        public ITeamIdentitySet Mentioned { get => GetValue<ITeamIdentitySet>(); set => SetValue(value); }
    }
}
