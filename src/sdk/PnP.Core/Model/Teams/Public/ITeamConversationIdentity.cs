using PnP.Core.Model.Security;

namespace PnP.Core.Model.Teams
{
    /// <summary>
    /// Represents a tag in Microsoft Teams. Tags allow users to quickly connect to subset of users in a team.
    /// </summary>
    [ConcreteType(typeof(TeamConversationIdentity))]
    public interface ITeamConversationIdentity : IDataModel<ITeamConversationIdentity>
    {
        /// <summary>
        /// Display name of the conversation. Optional.
        /// </summary>
        public string DisplayName{ get; }

        /// <summary>
        /// ID of the conversation.
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Type of conversation. Possible values are: team, channel, and chat.
        /// </summary>
        public TeamConversationIdentityType ConversationIdentityType { get; set; }
    }
}
