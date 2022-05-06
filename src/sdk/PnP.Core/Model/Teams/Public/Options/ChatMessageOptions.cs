using System.Collections.Generic;

namespace PnP.Core.Model.Teams
{
    /// <summary>
    /// Chat Message options
    /// </summary>
    public class ChatMessageOptions
    {
        /// <summary>
        /// Message content
        /// </summary>
        public string Content { get; set; }

        /// <summary>
        /// Type of the message content
        /// </summary>
        public ChatMessageContentType ContentType { get; set; } = ChatMessageContentType.Text;

        /// <summary>
        /// Message Subject
        /// </summary>
        public string Subject { get; set; }

        /// <summary>
        /// Attachment options
        /// </summary>
        public List<ChatMessageAttachmentOptions> Attachments { get; private set; } = new List<ChatMessageAttachmentOptions>();

        /// <summary>
        /// Message hosted content options
        /// </summary>
        public List<ChatMessageHostedContentOptions> HostedContents { get; private set; } = new List<ChatMessageHostedContentOptions>();

        /// <summary>
        /// Mention options
        /// </summary>
        public List<ChatMessageMentionOptions> Mentions { get; private set; } = new List<ChatMessageMentionOptions>();
    }
}
