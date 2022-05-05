namespace PnP.Core.Model.Teams
{
    /// <summary>
    /// 
    /// </summary>
    public class ChatMessageMentionOptions
    {
        /// <summary>
        /// Index of an entity being mentioned in the specified chatMessage. 
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// String used to represent the mention. For example, a user's display name, a team name.
        /// </summary>
        public string MentionText { get; set; }

        /// <summary>
        /// The entity (user, application, team, or channel) that was mentioned. If it was a channel or team that was @mentioned, 
        /// the identitySet contains a conversation property giving the ID of the team/channel, and a conversationIdentityType 
        /// property that represents either the team or channel.
        /// </summary>
        public ITeamChatMessageMentionedIdentitySet Mentioned { get; set; }
    }
}
