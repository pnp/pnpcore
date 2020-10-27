namespace PnP.Core.Model.Teams
{
    /// <summary>
    /// Public interface to define the memebers settings for a Team
    /// </summary>
    [ConcreteType(typeof(TeamMembersSettings))]
    public interface ITeamMembersSettings : IDataModel<ITeamMembersSettings>
    {
        /// <summary>
        /// Defines whether the team members can update channels
        /// </summary>
        public bool AllowCreateUpdateChannels { get; set; }

        /// <summary>
        /// Defines whether the team members can create private channels
        /// </summary>
        public bool AllowCreatePrivateChannels { get; set; }

        /// <summary>
        /// Defines whether the team members can delete channels
        /// </summary>
        public bool AllowDeleteChannels { get; set; }

        /// <summary>
        /// Defines whether the team members can add or remove apps
        /// </summary>
        public bool AllowAddRemoveApps { get; set; }

        /// <summary>
        /// Defines whether the team members can update or remove tabs
        /// </summary>
        public bool AllowCreateUpdateRemoveTabs { get; set; }

        /// <summary>
        /// Defines whether the team members can create, update, or remove connectors
        /// </summary>
        public bool AllowCreateUpdateRemoveConnectors { get; set; }
    }
}
