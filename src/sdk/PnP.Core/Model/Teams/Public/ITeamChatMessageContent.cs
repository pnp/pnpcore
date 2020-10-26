namespace PnP.Core.Model.Teams
{
    /// <summary>
    /// Public interface to define the content of a chat message
    /// </summary>
    [ConcreteType(typeof(TeamChatMessageContent))]
    public interface ITeamChatMessageContent : IDataModel<ITeamChatMessageContent>
    {
        /// <summary>
        /// The content of the item.
        /// </summary>
        public string Content { get; set; }

        /// <summary>
        /// The type of the content. Possible values are text and html.
        /// </summary>
        public ChatMessageContentType ContentType { get; set; }
    }
}
