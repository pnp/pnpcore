namespace PnP.Core.Model.Teams
{
    /// <summary>
    /// Public interface to define the discovery settings for a Team
    /// </summary>
    public interface ITeamDiscoverySettings: IComplexType
    {
        public bool ShowInTeamsSearchAndSuggestions { get; set; }
    }
}
