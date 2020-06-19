namespace PnP.Core.Model.Teams
{
    /// <summary>
    /// Defines a Microsoft Teams App installation
    /// </summary>
    [ConcreteType(typeof(TeamApp))]
    public interface ITeamApp : IDataModel<ITeamApp>
    {
        /// <summary>
        /// The unique Id of the Team App in the current app catalog
        /// </summary>
        public string Id { get; }

        /// <summary>
        /// The external ID of the Team App as defined by the developer
        /// </summary>
        public string ExternalId { get; set; }

        /// <summary>
        /// The Display Name of the Team App
        /// </summary>
        public string DisplayName { get; set; }

        /// <summary>
        /// The method of distribution for the Team App
        /// </summary>
        public TeamsAppDistributionMethod DistributionMethod { get; set; }

        // Note: we intentionally left out the collection of TeamsAppDefinition from this prototype
    }

    public enum TeamsAppDistributionMethod
    {
        Store,
        Organization,
        Sideloaded
    }
}
