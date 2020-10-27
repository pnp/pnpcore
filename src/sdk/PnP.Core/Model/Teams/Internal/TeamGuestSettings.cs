namespace PnP.Core.Model.Teams
{
    [GraphType]
    internal partial class TeamGuestSettings : BaseDataModel<ITeamGuestSettings>, ITeamGuestSettings
    {
        #region Properties
        public bool AllowCreateUpdateChannels { get => GetValue<bool>(); set => SetValue(value); }
        public bool AllowDeleteChannels { get => GetValue<bool>(); set => SetValue(value); }
        #endregion
    }
}
