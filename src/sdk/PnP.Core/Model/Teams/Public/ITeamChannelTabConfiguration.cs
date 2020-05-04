namespace PnP.Core.Model.Teams
{
    /// <summary>
    /// Public interface to define the configuration settings for a Team tab
    /// </summary>
    public interface ITeamChannelTabConfiguration : IComplexType
    {
        /// <summary>
        /// Identifier for the entity hosted by the tab provider.
        /// </summary>
        public string EntityId { get; set; }

        /// <summary>
        /// Url used for rendering tab contents in Teams. Required.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1056:Uri properties should not be strings", Justification = "<Pending>")]
        public string ContentUrl { get; set; }

        /// <summary>
        /// Url called by Teams client when a Tab is removed using the Teams Client.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1056:Uri properties should not be strings", Justification = "<Pending>")]
        public string RemoveUrl { get; set; }

        /// <summary>
        /// Url for showing tab contents outside of Teams.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1056:Uri properties should not be strings", Justification = "<Pending>")]
        public string WebsiteUrl { get; set; }

        /// <summary>
        /// Is there content in this tab.
        /// </summary>
        public bool HasContent { get; set; }

        /// <summary>
        /// Wiki tab id.
        /// </summary>
        public int WikiTabId { get; set; }

        /// <summary>
        /// Is this the default wiki tab.
        /// </summary>
        public bool WikiDefaultTab { get; set; }

    }
}
