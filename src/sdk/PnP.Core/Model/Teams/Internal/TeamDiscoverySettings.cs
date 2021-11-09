namespace PnP.Core.Model.Teams
{
    [GraphType]
    internal sealed class TeamDiscoverySettings : BaseDataModel<ITeamDiscoverySettings>, ITeamDiscoverySettings
    {
        #region Properties
        public bool ShowInTeamsSearchAndSuggestions { get => GetValue<bool>(); set => SetValue(value); }
        #endregion
    }
}
