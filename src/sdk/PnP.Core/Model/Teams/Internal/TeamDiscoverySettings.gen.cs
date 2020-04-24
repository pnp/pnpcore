namespace PnP.Core.Model.Teams
{
    [GraphType]
    internal partial class TeamDiscoverySettings : BaseComplexType<ITeamDiscoverySettings>, ITeamDiscoverySettings
    {
        public bool ShowInTeamsSearchAndSuggestions { get => GetValue<bool>(); set => SetValue(value); }
    }
}
