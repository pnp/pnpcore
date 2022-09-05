namespace PnP.Core.Model.Teams
{
    [GraphType]
    internal sealed class TeamChatMessageMention : BaseDataModel<ITeamChatMessageMention>, ITeamChatMessageMention
    {
        #region Properties
        public int Id { get => GetValue<int>(); set => SetValue(value); }

        public string MentionText { get => GetValue<string>(); set => SetValue(value); }

        public ITeamChatMessageMentionedIdentitySet Mentioned { get => GetModelValue<ITeamChatMessageMentionedIdentitySet>(); set => SetModelValue(value); }

        [KeyProperty(nameof(Id))]
        public override object Key { get => Id; set => Id = int.Parse(value.ToString()); }
        #endregion
    }
}
