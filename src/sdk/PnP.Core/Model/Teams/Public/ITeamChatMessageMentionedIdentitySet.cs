using PnP.Core.Model.Security;

namespace PnP.Core.Model.Teams
{
    /// <summary>
    /// Represents the resource (user, application, or conversation) @mentioned in a message in a chat or a channel.
    /// </summary>
    [ConcreteType(typeof(TeamChatMessageMentionedIdentitySet))]
    public interface ITeamChatMessageMentionedIdentitySet : IDataModel<ITeamChatMessageMentionedIdentitySet>
    {
        /// <summary>
        /// If present, represents an application (for example, bot) @mentioned in a message.
        /// </summary>
        public IIdentity Application { get; set; }

        /// <summary>
        /// If present, represents a conversation (for example, team or channel) @mentioned in a message.
        /// </summary>
        public ITeamConversationIdentity Conversation { get; set; }

        /// <summary>
        /// If present, represents a user @mentioned in a message.
        /// </summary>
        public IIdentity User { get; set; }

        /// <summary>
        /// If present, represents a tag @mentioned in a team message.
        /// </summary>
        public ITeamTagIdentity Tag { get; set;}
    }
}
