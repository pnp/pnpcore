using PnP.Core.Services;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace PnP.Core.Model.Teams
{
    /// <summary>
    /// Represents an individual chat message within a channel or chat. The chat message can be a root chat message or 
    /// part of a reply thread that is defined by the replyToId property in the chat message.
    /// </summary>
    [ConcreteType(typeof(TeamChatMessage))]
    public interface ITeamChatMessage : IDataModel<ITeamChatMessage>, IDataModelGet<ITeamChatMessage>, IDataModelLoad<ITeamChatMessage>, IQueryableDataModel
    {

        /// <summary>
        /// Read-only. Unique Id of the message.
        /// </summary>
        public string Id { get; }

        /// <summary>
        /// Read-only. Id of the parent chat message or root chat message of the thread. (Only applies to chat messages in channels not chats)
        /// </summary>
        public string ReplyToId { get; }

        /// <summary>
        /// Read only. Details of the sender of the chat message.
        /// </summary>
        public ITeamIdentitySet From { get; }

        /// <summary>
        /// Read-only. Version number of the chat message.
        /// </summary>
        public string Etag { get; }

        /// <summary>
        /// The type of chat message. The possible values are: message.
        /// </summary>
        public ChatMessageType MessageType { get; set; }

        /// <summary>
        /// Read only. Timestamp of when the chat message was created.
        /// </summary>
        public DateTimeOffset CreatedDateTime { get; }

        /// <summary>
        /// Read only. Timestamp of when the chat message is created or edited, including when a reply is made (if it's a root chat message in a channel) or a reaction is added or removed.
        /// </summary>
        public DateTimeOffset LastModifiedDateTime { get; }

        /// <summary>
        /// Read only. Timestamp at which the chat message was deleted, or null if not deleted.
        /// </summary>
        public DateTimeOffset DeletedDateTime { get; }

        /// <summary>
        /// The subject of the chat message, in plaintext.
        /// </summary>
        public string Subject { get; set; }

        /// <summary>
        /// Plaintext/HTML representation of the content of the chat message. Representation is specified by the contentType inside the body. 
        /// The content is always in HTML if the chat message contains a chatMessageMention.
        /// </summary>
        public ITeamChatMessageContent Body { get; }

        /// <summary>
        /// Channel identity reference
        /// </summary>
        public ITeamChannelIdentity ChannelIdentity { get; }

        /// <summary>
        /// Summary text of the chat message that could be used for push notifications and summary views or fall back views. 
        /// Only applies to channel chat messages, not chat messages in a chat.
        /// </summary>
        public string Summary { get; set; }

        /// <summary>
        /// The importance of the chat message.
        /// </summary>
        public ChatMessageImportance Importance { get; set; }

        /// <summary>
        /// The Web URL of the team chat message
        /// </summary>
        public Uri WebUrl { get; }

        /// <summary>
        /// Locale of the team chat message
        /// </summary>
        public string Locale { get; set; }

        /// <summary>
        /// Reactions for this chat message (for example, Like).
        /// </summary>
        public ITeamChatMessageReactionCollection Reactions { get; }

        /// <summary>
        /// List of entities mentioned in the chat message. Currently supports user, bot, team, channel.
        /// </summary>
        public ITeamChatMessageMentionCollection Mentions { get; }

        /// <summary>
        /// Attached files
        /// </summary>
        public ITeamChatMessageAttachmentCollection Attachments { get; }

        /// <summary>
        /// Hosted Content tiles
        /// </summary>
        public ITeamChatMessageHostedContentCollection HostedContents { get; }

        /// <summary>
        /// Collection of replies for a message
        /// Implements <see cref="IQueryable{T}"/>. <br />
        /// See <see href="https://pnp.github.io/pnpcore/using-the-sdk/basics-getdata.html#requesting-model-collections">Requesting model collections</see> 
        /// and <see href="https://pnp.github.io/pnpcore/using-the-sdk/basics-iqueryable.html">IQueryable performance considerations</see> to learn more.
        /// </summary>
        public ITeamChatMessageReplyCollection Replies { get; }

        #region Methods

        /// <summary>
        /// Adds a reply to an existing message
        /// </summary>
        /// <param name="options">Options for the reply to create</param>
        /// <returns>Newly added reply</returns>
        public Task<ITeamChatMessageReply> AddReplyAsync(ChatMessageOptions options);

        /// <summary>
        /// Adds a reply to an existing message
        /// </summary>
        /// <param name="options">Options for the reply to create</param>
        /// <returns>Newly added reply</returns>
        public ITeamChatMessageReply AddReply(ChatMessageOptions options);

        /// <summary>
        /// Adds a reply to an existing message
        /// </summary>
        /// <param name="batch">Batch the reply is associated with</param>
        /// <param name="options">Options for the reply to create</param>
        /// <returns>Newly added reply</returns>
        public Task<ITeamChatMessageReply> AddReplyBatchAsync(Batch batch, ChatMessageOptions options);

        /// <summary>
        /// Adds a reply to an existing message
        /// </summary>
        /// <param name="batch">Batch the reply is associated with</param>
        /// <param name="options">Options for the reply to create</param>
        /// <returns>Newly added reply</returns>
        public ITeamChatMessageReply AddReplyBatch(Batch batch, ChatMessageOptions options);

        /// <summary>
        /// Adds a reply to an existing message
        /// </summary>
        /// <param name="options">Options for the reply to create</param>
        /// <returns>Newly added reply</returns>
        public Task<ITeamChatMessageReply> AddReplyBatchAsync(ChatMessageOptions options);

        /// <summary>
        /// Adds a reply to an existing message
        /// </summary>
        /// <param name="options">Options for the reply to create</param>
        /// <returns>Newly added reply</returns>
        public ITeamChatMessageReply AddReplyBatch(ChatMessageOptions options);

        /// <summary>
        /// Adds a reply to an existing message
        /// </summary>
        /// <param name="content">Content of the message</param>
        /// <param name="contentType">Message content type e.g. Text, Html</param>
        /// <param name="subject">Message Subject</param>
        /// <returns>Newly added reply</returns>
        public Task<ITeamChatMessageReply> AddReplyAsync(string content, ChatMessageContentType contentType = ChatMessageContentType.Text, string subject = null);

        /// <summary>
        /// Adds a reply to an existing message
        /// </summary>
        /// <param name="content">Content of the reply</param>
        /// <param name="contentType">Reply content type e.g. Text, Html</param>
        /// <param name="subject">Reply Subject</param>
        /// <returns>Newly added reply</returns>
        public ITeamChatMessageReply AddReply(string content, ChatMessageContentType contentType = ChatMessageContentType.Text, string subject = null);

        /// <summary>
        /// Adds a reply to an existing message
        /// </summary>
        /// <param name="batch">Batch the reply is associated with</param>
        /// <param name="content">Content of the reply</param>
        /// <param name="contentType">Reply content type e.g. Text, Html</param>
        /// <param name="subject">Reply Subject</param>
        /// <returns>Newly added reply</returns>
        public Task<ITeamChatMessageReply> AddReplyBatchAsync(Batch batch, string content, ChatMessageContentType contentType = ChatMessageContentType.Text, string subject = null);

        /// <summary>
        /// Adds a reply to an existing message
        /// </summary>
        /// <param name="batch">Batch the reply is associated with</param>
        /// <param name="content">Content of the reply</param>
        /// <param name="contentType">Reply content type e.g. Text, Html</param>
        /// <param name="subject">Reply Subject</param>
        /// <returns>Newly added reply</returns>
        public ITeamChatMessageReply AddReplyBatch(Batch batch, string content, ChatMessageContentType contentType = ChatMessageContentType.Text, string subject = null);

        /// <summary>
        /// Adds a reply to an existing message
        /// </summary>
        /// <param name="content">Content of the Reply</param>
        /// <param name="contentType">Reply content type e.g. Text, Html</param>
        /// <param name="subject">Reply Subject</param>
        /// <returns>Newly added reply</returns>
        public Task<ITeamChatMessageReply> AddReplyBatchAsync(string content, ChatMessageContentType contentType = ChatMessageContentType.Text, string subject = null);

        /// <summary>
        /// Adds a reply to an existing message
        /// </summary>
        /// <param name="content">Content of the Reply</param>
        /// <param name="contentType">Reply content type e.g. Text, Html</param>
        /// <param name="subject">Reply Subject</param>
        /// <returns>Newly added reply</returns>
        public ITeamChatMessageReply AddReplyBatch(string content, ChatMessageContentType contentType = ChatMessageContentType.Text, string subject = null);

        #endregion

    }
}
