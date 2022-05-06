using System;

namespace PnP.Core.Model.Teams
{
    /// <summary>
    /// Represents a tag associated with a team.
    /// </summary>
    [ConcreteType(typeof(TeamTag))]
    public interface ITeamTag : IDataModel<ITeamTag>, IDataModelDelete, IDataModelUpdate
    {
        /// <summary>
        /// Tag description as it will appear to the user in Microsoft Teams.
        /// </summary>
        public string Description { get; }

        /// <summary>
        /// Name of the tag. The value can't be more than 40 characters.
        /// </summary>
        public string DisplayName { get; set; }

        /// <summary>
        /// ID of the tag.
        /// </summary>
        public string Id { get; }

        /// <summary>
        /// The number of users assigned to the tag.
        /// </summary>
        public int MemberCount { get; }

        /// <summary>
        /// ID of the team in which the tag is defined.
        /// </summary>
        public string TeamId { get; }

        /// <summary>
        /// Tag description as it will appear to the user in Microsoft Teams.
        /// </summary>
        public TeamTagType TagType { get; set; }

        /// <summary>
        /// Members that are associated with the Teams tag
        /// </summary>
        public ITeamTagMemberCollection Members { get; set; }
    }
}
