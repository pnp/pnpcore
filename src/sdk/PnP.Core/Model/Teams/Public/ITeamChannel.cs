using System;

namespace PnP.Core.Model.Teams
{
    /// <summary>
    /// Defines a Channel of Microsoft Teams
    /// </summary>
    [ConcreteType(typeof(TeamChannel))]
    public interface ITeamChannel : IDataModel<ITeamChannel>, IDataModelUpdate, IDataModelDelete, IQueryableDataModel
    {
        /// <summary>
        /// The Unique ID of the Team Channel
        /// </summary>
        public string Id { get; }

        /// <summary>
        /// The display name of the Team Channel
        /// </summary>
        public string DisplayName { get; set; }

        /// <summary>
        /// The description of the Team Channel
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Defines whether the Team Channel is favorite by default in the Team
        /// </summary>
        public bool IsFavoriteByDefault { get; set; }

        /// <summary>
        /// The email address of the Team Channel
        /// </summary>
        public string Email { get; }

        /// <summary>
        /// Defines the Membership type for the Team Channel
        /// </summary>
        public TeamChannelMembershipType MembershipType { get; set; }

        /// <summary>
        /// The Web URL of the Team Channel
        /// </summary>
        public Uri WebUrl { get; }

        /// <summary>
        /// Tabs in this Channel
        /// </summary>
        public ITeamChannelTabCollection Tabs { get; }

        /// <summary>
        /// Messages in this Team Channel
        /// </summary>
        public ITeamChatMessageCollection Messages { get; }
    }
}
