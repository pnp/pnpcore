using PnP.Core.Model.Teams;

namespace PnP.Core.Model.Me
{
    /// <summary>
    /// Public interface to define the content of a chat message
    /// </summary>
    [ConcreteType(typeof(ChatMessageContent))]
    public interface IChatMessageContent : IDataModel<IChatMessageContent>
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
