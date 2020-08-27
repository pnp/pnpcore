namespace PnP.Core.Model.Teams
{
    /// <summary>
    /// Public interface to define the discovery settings for a Team
    /// </summary>
    public interface ITeamDiscoverySettings: IComplexType
    {
        /// <summary>
        /// Show team in Teams search and suggestions?
        /// </summary>
        public bool ShowInTeamsSearchAndSuggestions { get; set; }
    }
}
