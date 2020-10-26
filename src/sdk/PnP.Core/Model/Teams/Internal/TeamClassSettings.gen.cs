namespace PnP.Core.Model.Teams
{
    [GraphType]
    internal partial class TeamClassSettings : BaseDataModel<ITeamClassSettings>, ITeamClassSettings
    {
        public bool NotifyGuardiansAboutAssignments { get => GetValue<bool>(); set => SetValue(value); }
    }
}
