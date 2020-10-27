namespace PnP.Core.Model.Teams
{
    /// <summary>
    /// Public interface to define the discovery settings for a Team
    /// </summary>
    [ConcreteType(typeof(TeamDiscoverySettings))]
    public interface ITeamDiscoverySettings : IDataModel<ITeamDiscoverySettings>
    {
        /// <summary>
        /// Show team in Teams search and suggestions?
        /// </summary>
        public bool ShowInTeamsSearchAndSuggestions { get; set; }
    }
}
