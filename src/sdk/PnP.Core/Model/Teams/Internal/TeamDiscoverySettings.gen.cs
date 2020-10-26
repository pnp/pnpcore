namespace PnP.Core.Model.Teams
{
    [GraphType]
    internal partial class TeamDiscoverySettings : BaseDataModel<ITeamDiscoverySettings>, ITeamDiscoverySettings
    {
        public bool ShowInTeamsSearchAndSuggestions { get => GetValue<bool>(); set => SetValue(value); }
    }
}
