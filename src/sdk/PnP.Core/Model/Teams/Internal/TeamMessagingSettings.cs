namespace PnP.Core.Model.Teams
{
    [GraphType]
    internal sealed class TeamMessagingSettings : BaseDataModel<ITeamMessagingSettings>, ITeamMessagingSettings
    {
        #region Properties
        public bool AllowUserEditMessages { get => GetValue<bool>(); set => SetValue(value); }

        public bool AllowUserDeleteMessages { get => GetValue<bool>(); set => SetValue(value); }

        public bool AllowOwnerDeleteMessages { get => GetValue<bool>(); set => SetValue(value); }

        public bool AllowTeamMentions { get => GetValue<bool>(); set => SetValue(value); }

        public bool AllowChannelMentions { get => GetValue<bool>(); set => SetValue(value); }
        #endregion
    }
}
