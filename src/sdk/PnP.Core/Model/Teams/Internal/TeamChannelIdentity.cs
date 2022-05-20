namespace PnP.Core.Model.Teams
{
    [GraphType]
    internal sealed class TeamChannelIdentity : BaseDataModel<ITeamChannelIdentity>, ITeamChannelIdentity
    {
        #region Properties

        public string ChannelId { get => GetValue<string>(); set => SetValue(value); }

        public string TeamId { get => GetValue<string>(); set => SetValue(value); }
       
        #endregion
    }
}
