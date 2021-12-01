namespace PnP.Core.Model.Teams
{
    [GraphType]
    internal sealed class TeamClassSettings : BaseDataModel<ITeamClassSettings>, ITeamClassSettings
    {
        #region Properties
        public bool NotifyGuardiansAboutAssignments { get => GetValue<bool>(); set => SetValue(value); }
        #endregion
    }
}
