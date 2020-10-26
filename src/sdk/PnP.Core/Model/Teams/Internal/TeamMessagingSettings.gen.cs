namespace PnP.Core.Model.Teams
{
    [GraphType]
    internal partial class TeamMessagingSettings : BaseDataModel<ITeamMessagingSettings>, ITeamMessagingSettings
    {
        public bool AllowUserEditMessages { get => GetValue<bool>(); set => SetValue(value); }
        public bool AllowUserDeleteMessages { get => GetValue<bool>(); set => SetValue(value); }
        public bool AllowOwnerDeleteMessages { get => GetValue<bool>(); set => SetValue(value); }
        public bool AllowTeamMentions { get => GetValue<bool>(); set => SetValue(value); }
        public bool AllowChannelMentions { get => GetValue<bool>(); set => SetValue(value); }
    }
}
