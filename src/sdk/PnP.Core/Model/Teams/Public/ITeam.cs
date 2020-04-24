using PnP.Core.Model.AzureActiveDirectory;
using System;

namespace PnP.Core.Model.Teams
{
    /// <summary>
    /// Public interface to define a Team object of Microsoft Teams
    /// </summary>
    public interface ITeam : IDataModel<ITeam>, IDataModelUpdate
    {
        /// <summary>
        /// The Unique ID of the Team/Group
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// The display name of the Team
        /// </summary>
        public string DisplayName { get; set; }

        /// <summary>
        /// The description of the Team
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// The Internal ID of the Team
        /// </summary>
        public string InternalId { get; set; }

        /// <summary>
        /// The classification of the Team
        /// </summary>
        public string Classification { get; set; }

        /// <summary>
        /// The specialization of the Team
        /// </summary>
        public TeamSpecialization Specialization { get; set; }

        /// <summary>
        /// The specialization of the Team
        /// </summary>
        public TeamVisibility Visibility { get; set; }

        /// <summary>
        /// The Web URL of the Team
        /// </summary>
        public Uri WebUrl { get; set; }

        /// <summary>
        /// Defines whether the Team is archived or not
        /// </summary>
        public bool IsArchived { get; set; }

        /// <summary>
        /// Defines the Members Settings for the Team
        /// </summary>
        public ITeamMembersSettings MemberSettings { get; set; }

        /// <summary>
        /// Defines the Guest Settings for the Team
        /// </summary>
        public ITeamGuestSettings GuestSettings { get; set; }

        /// <summary>
        /// Defines the Members Settings for the Team
        /// </summary>
        public ITeamMessagingSettings MessagingSettings { get; set; }

        /// <summary>
        /// Defines the Members Settings for the Team
        /// </summary>
        public ITeamFunSettings FunSettings { get; set; }

        /// <summary>
        /// Defines the Members Settings for the Team
        /// </summary>
        public ITeamDiscoverySettings DiscoverySettings { get; set; }

        /// <summary>
        /// Defines the Class Settings for the Team
        /// </summary>
        public ITeamClassSettings ClassSettings { get; set; }

        /// <summary>
        /// Reference to the Primary Channel for the Team
        /// </summary>
        public ITeamChannel PrimaryChannel { get; set; }

        /// <summary>
        /// Collection of channels in this current Team
        /// </summary>
        public ITeamChannelCollection Channels { get; }

        /// <summary>
        /// Collection of installed apps in this current Team
        /// </summary>
        public ITeamAppCollection InstalledApps { get; }

        /// <summary>
        /// Collection of Owners of the current Team
        /// </summary>
        public IUserCollection Owners { get; }

        /// <summary>
        /// Collection of Members of the current Team
        /// </summary>
        public IUserCollection Members { get; }

        // Note: so far, we intentionally skipped the following properties
        // - operations
        // - schedule
        // - template
    }
}
