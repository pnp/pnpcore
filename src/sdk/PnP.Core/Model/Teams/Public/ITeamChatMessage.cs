using System;
using System.Collections.Generic;

namespace PnP.Core.Model.Teams
{
    /// <summary>
    /// Represents an individual chat message within a channel or chat. The chat message can be a root chat message or 
    /// part of a reply thread that is defined by the replyToId property in the chat message.
    /// </summary>
    [ConcreteType(typeof(TeamChatMessage))]
    public interface ITeamChatMessage : IDataModel<ITeamChatMessage>, IQueryableModel
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
        public List<ITeamChatMessageReaction> Reactions { get; }

        /// <summary>
        /// List of entities mentioned in the chat message. Currently supports user, bot, team, channel.
        /// </summary>
        public List<ITeamChatMessageMention> Mentions { get; }

        /// <summary>
        /// Attached files. Attachments are currently read-only – sending attachments is not supported.
        /// </summary>
        public List<ITeamChatMessageAttachment> Attachments { get; }

    }
}
