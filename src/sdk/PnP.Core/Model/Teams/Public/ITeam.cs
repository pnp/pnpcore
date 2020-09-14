using PnP.Core.Model.Security;
using System;
using System.Threading.Tasks;

namespace PnP.Core.Model.Teams
{
    /// <summary>
    /// Public interface to define a Team object of Microsoft Teams
    /// </summary>
    [ConcreteType(typeof(Team))]
    public interface ITeam : IDataModel<ITeam>, IDataModelUpdate
    {
        /// <summary>
        /// The Unique ID of the Team/Group
        /// </summary>
        public Guid Id { get; }

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
        public string InternalId { get; }

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
        public Uri WebUrl { get; }

        /// <summary>
        /// Defines whether the Team is archived or not
        /// </summary>
        public bool IsArchived { get; }

        /// <summary>
        /// Defines the Members Settings for the Team
        /// </summary>
        public ITeamMembersSettings MemberSettings { get; }

        /// <summary>
        /// Defines the Guest Settings for the Team
        /// </summary>
        public ITeamGuestSettings GuestSettings { get; }

        /// <summary>
        /// Defines the Members Settings for the Team
        /// </summary>
        public ITeamMessagingSettings MessagingSettings { get; }

        /// <summary>
        /// Defines the Members Settings for the Team
        /// </summary>
        public ITeamFunSettings FunSettings { get; }

        /// <summary>
        /// Defines the Members Settings for the Team
        /// </summary>
        public ITeamDiscoverySettings DiscoverySettings { get; }

        /// <summary>
        /// Defines the Class Settings for the Team
        /// </summary>
        public ITeamClassSettings ClassSettings { get; }

        /// <summary>
        /// Reference to the Primary Channel for the Team
        /// </summary>
        public ITeamChannel PrimaryChannel { get; }

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
        public IGraphUserCollection Owners { get; }

        /// <summary>
        /// Collection of Members of the current Team
        /// </summary>
        public IGraphUserCollection Members { get; }

        // Note: so far, we intentionally skipped the following properties
        // - operations
        // - schedule
        // - template

        /// <summary>
        /// Archives the team
        /// </summary>
        /// <returns></returns>
        public Task<ITeamAsyncOperation> ArchiveAsync();

        /// <summary>
        /// Archives the team
        /// </summary>
        /// <returns></returns>
        public ITeamAsyncOperation Archive();

        /// <summary>
        /// Archives the team
        /// </summary>
        /// <param name="setSPOSiteReadOnlyForMembers">This optional parameter defines whether to set permissions for team members to read-only on the Sharepoint Online site associated with the team</param>
        /// <returns></returns>
        public Task<ITeamAsyncOperation> ArchiveAsync(bool setSPOSiteReadOnlyForMembers);

        /// <summary>
        /// Archives the team
        /// </summary>
        /// <param name="setSPOSiteReadOnlyForMembers">This optional parameter defines whether to set permissions for team members to read-only on the Sharepoint Online site associated with the team</param>
        /// <returns></returns>
        public ITeamAsyncOperation Archive(bool setSPOSiteReadOnlyForMembers);

        /// <summary>
        /// Unarchives the team
        /// </summary>
        /// <returns></returns>
        public Task<ITeamAsyncOperation> UnarchiveAsync();

        /// <summary>
        /// Unarchives the team
        /// </summary>
        /// <returns></returns>
        public ITeamAsyncOperation Unarchive();
    }
}
