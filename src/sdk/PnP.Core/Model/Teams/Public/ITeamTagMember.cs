using System;

namespace PnP.Core.Model.Teams
{
    /// <summary>
    /// Represents a tag associated with a team.
    /// </summary>
    [ConcreteType(typeof(TeamTagMember))]
    public interface ITeamTagMember : IDataModel<ITeamTagMember>
    {
        /// <summary>
        /// The member's display name.
        /// </summary>
        public string DisplayName { get; }

        /// <summary>
        /// The ID of the tenant that the tag member is a part of.
        /// </summary>
        public string TenantId { get; set; }

        /// <summary>
        /// ID of the member.
        /// </summary>
        public string Id { get; }

        /// <summary>
        /// The user ID of the member.
        /// </summary>
        public string UserId { get; }
    }
}
