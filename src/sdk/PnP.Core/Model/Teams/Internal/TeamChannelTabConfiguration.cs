namespace PnP.Core.Model.Teams
{
    [GraphType]
    internal partial class TeamChannelTabConfiguration : BaseDataModel<ITeamChannelTabConfiguration>, ITeamChannelTabConfiguration
    {
        #region Properties
        public string EntityId { get => GetValue<string>(); set => SetValue(value); }

        public string ContentUrl { get => GetValue<string>(); set => SetValue(value); }

        public string RemoveUrl { get => GetValue<string>(); set => SetValue(value); }

        public string WebsiteUrl { get => GetValue<string>(); set => SetValue(value); }

        public bool HasContent { get => GetValue<bool>(); set => SetValue(value); }

        public int WikiTabId { get => GetValue<int>(); set => SetValue(value); }

        public bool WikiDefaultTab { get => GetValue<bool>(); set => SetValue(value); }
        #endregion
    }
}
