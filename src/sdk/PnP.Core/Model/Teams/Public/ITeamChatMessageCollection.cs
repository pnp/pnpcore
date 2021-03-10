using PnP.Core.Services;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PnP.Core.Model.Teams
{
    /// <summary>
    /// A chat is a collection of chatMessages between one or more participants. Participants can be users or apps.
    /// </summary>
    [ConcreteType(typeof(TeamChatMessageCollection))]
    public interface ITeamChatMessageCollection : IQueryable<ITeamChatMessage>, IAsyncEnumerable<ITeamChatMessage>, IDataModelCollectionLoad<ITeamChatMessage>, IDataModelCollection<ITeamChatMessage>
    {
        /// <summary>
        /// Adds a new channel chat message
        /// </summary>
        /// <param name="content">Content of the message</param>
        /// <param name="contentType">Message content type e.g. Text, Html</param>
        /// <param name="attachments">Attachments within the message</param>
        /// <param name="subject">Message subject</param>
        /// <param name="hostedContents">Hosted contents for inline content</param>
        /// <returns></returns>
        public Task<ITeamChatMessage> AddAsync(string content, ChatMessageContentType contentType = ChatMessageContentType.Text, ITeamChatMessageAttachmentCollection attachments = null, string subject = null, ITeamChatMessageHostedContentCollection hostedContents = null);

        /// <summary>
        /// Adds a new channel chat
        /// </summary>
        /// <param name="content">Content of the message</param>
        /// <param name="contentType">Message content type e.g. Text, Html</param>
        /// <param name="attachments">Attachments within the message</param>
        /// <param name="subject">Message subject</param>
        /// <param name="hostedContents">Hosted contents for inline content</param>
        /// <returns></returns>
        public ITeamChatMessage Add(string content, ChatMessageContentType contentType = ChatMessageContentType.Text, ITeamChatMessageAttachmentCollection attachments = null, string subject = null, ITeamChatMessageHostedContentCollection hostedContents = null);

        /// <summary>
        /// Adds a new channel chat message
        /// </summary>
        /// <param name="batch">Batch the message is associated with</param>
        /// <param name="content">Content of the message</param>
        /// <param name="contentType">Message content type e.g. Text, Html</param>
        /// <param name="attachments">Attachments within the message</param>
        /// <param name="subject">Message subject</param>
        /// <param name="hostedContents">Hosted contents for inline content</param>
        /// <returns>Newly added channel chat message</returns>
        public Task<ITeamChatMessage> AddBatchAsync(Batch batch, string content, ChatMessageContentType contentType = ChatMessageContentType.Text, ITeamChatMessageAttachmentCollection attachments = null, string subject = null, ITeamChatMessageHostedContentCollection hostedContents = null);

        /// <summary>
        /// Adds a new channel chat message
        /// </summary>
        /// <param name="batch">Batch the message is associated with</param>
        /// <param name="content">Content of the message</param>
        /// <param name="contentType">Message content type e.g. Text, Html</param>
        /// <param name="attachments">Attachments within the message</param>
        /// <param name="subject">Message subject</param>
        /// <param name="hostedContents">Hosted contents for inline content</param>
        /// <returns>Newly added channel chat message</returns>
        public ITeamChatMessage AddBatch(Batch batch, string content, ChatMessageContentType contentType = ChatMessageContentType.Text, ITeamChatMessageAttachmentCollection attachments = null, string subject = null, ITeamChatMessageHostedContentCollection hostedContents = null);

        /// <summary>
        /// Adds a new channel chat message
        /// </summary>
        /// <param name="content">Content of the message</param>
        /// <param name="contentType">Message content type e.g. Text, Html</param>
        /// <param name="attachments">Attachments within the message</param>
        /// <param name="subject">Message subject</param>
        /// <param name="hostedContents">Hosted contents for inline content</param>
        /// <returns>Newly added channel chat message</returns>
        public Task<ITeamChatMessage> AddBatchAsync(string content, ChatMessageContentType contentType = ChatMessageContentType.Text, ITeamChatMessageAttachmentCollection attachments = null, string subject = null, ITeamChatMessageHostedContentCollection hostedContents = null);

        /// <summary>
        /// Adds a new channel chat message
        /// </summary>
        /// <param name="content">Content of the message</param>
        /// <param name="contentType">Message content type e.g. Text, Html</param>
        /// <param name="attachments">Attachments within the message</param>
        /// <param name="subject">Message subject</param>
        /// <param name="hostedContents">Hosted contents for inline content</param>
        /// <returns>Newly added channel chat message</returns>
        public ITeamChatMessage AddBatch(string content, ChatMessageContentType contentType = ChatMessageContentType.Text, ITeamChatMessageAttachmentCollection attachments = null, string subject = null, ITeamChatMessageHostedContentCollection hostedContents = null);

    }
}
