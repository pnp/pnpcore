using PnP.Core.QueryModel;
using PnP.Core.Services;
using System;
using System.Threading.Tasks;

namespace PnP.Core.Model.Teams
{
    internal partial class TeamChatMessageCollection : QueryableDataModelCollection<ITeamChatMessage>, ITeamChatMessageCollection
    {
        public TeamChatMessageCollection(PnPContext context, IDataModelParent parent, string memberName = null)
            : base(context, parent, memberName)
        {
            PnPContext = context;
            Parent = parent;
        }

        /// <summary>
        /// Adds new channel chat message with support for content types
        /// </summary>
        /// <param name="content">Content of the message</param>
        /// <param name="contentType">Message content type e.g. Text, Html</param>
        /// <param name="attachments">Attachments within the message</param>
        /// <param name="subject">Message subject</param>
        /// <returns></returns>
        public async Task<ITeamChatMessage> AddAsync(string content, ChatMessageContentType contentType = ChatMessageContentType.Text, ITeamChatMessageAttachmentCollection attachments = null, string subject = null)
        {
            if (string.IsNullOrEmpty(content))
            {
                throw new ArgumentNullException(nameof(content));
            }

            var newChannelChatMessage = CreateNewAndAdd() as TeamChatMessage;

            // Assign field values
            newChannelChatMessage.Body = new TeamChatMessageContent
            {
                PnPContext = newChannelChatMessage.PnPContext,
                Parent = newChannelChatMessage,
                Content = content,
                ContentType = contentType,
            };

            if(attachments != null && attachments.Length > 0)
            {
                newChannelChatMessage.Attachments = attachments;
            }


            return await newChannelChatMessage.AddAsync().ConfigureAwait(false) as TeamChatMessage;
        }

        /// <summary>
        /// Adds a new channel chat message
        /// </summary>
        /// <param name="content">Content of the message</param>
        /// <param name="contentType">Message content type e.g. Text, Html</param>
        /// <param name="attachments">Attachments within the message</param>
        /// <param name="subject">Message subject</param>
        /// <returns>Newly added channel chat message</returns>
        public ITeamChatMessage Add(string content, ChatMessageContentType contentType = ChatMessageContentType.Text, ITeamChatMessageAttachmentCollection attachments = null, string subject = null)
        {
            return AddAsync(content).GetAwaiter().GetResult();
        }

        /// <summary>
        /// Adds a new channel chat message
        /// </summary>
        /// <param name="batch">Batch the message is associated with</param>
        /// <param name="content">Content of the message</param>
        /// <param name="contentType">Message content type e.g. Text, Html</param>
        /// <param name="attachments">Attachments within the message</param>
        /// <param name="subject">Message subject</param>
        /// <returns>Newly added channel chat message</returns>
        public async Task<ITeamChatMessage> AddBatchAsync(Batch batch, string content, ChatMessageContentType contentType = ChatMessageContentType.Text, ITeamChatMessageAttachmentCollection attachments = null, string subject = null)
        {
            if (string.IsNullOrEmpty(content))
            {
                throw new ArgumentNullException(nameof(content));
            }

            var newChannelChatMessage = CreateNewAndAdd() as TeamChatMessage;

            // Assign field values
            newChannelChatMessage.Body = new TeamChatMessageContent
            {
                PnPContext = newChannelChatMessage.PnPContext,
                Parent = newChannelChatMessage,
                Content = content,
                ContentType = contentType
            };

            return await newChannelChatMessage.AddBatchAsync(batch).ConfigureAwait(false) as TeamChatMessage;
        }

        /// <summary>
        /// Adds a new channel chat message
        /// </summary>
        /// <param name="content">Content of the message</param>
        /// <param name="contentType">Message content type e.g. Text, Html</param>
        /// <param name="attachments">Attachments within the message</param>
        /// <param name="subject">Message subject</param>
        /// <returns>Newly added channel chat message</returns>
        public async Task<ITeamChatMessage> AddBatchAsync(string content, ChatMessageContentType contentType = ChatMessageContentType.Text, ITeamChatMessageAttachmentCollection attachments = null, string subject = null)
        {
            return await AddBatchAsync(PnPContext.CurrentBatch, content, contentType, attachments, subject).ConfigureAwait(false);
        }

        /// <summary>
        /// Adds a new channel chat message
        /// </summary>
        /// <param name="content">Content of the message</param>
        /// <param name="contentType">Message content type e.g. Text, Html</param>
        /// <param name="attachments">Attachments within the message</param>
        /// <param name="subject">Message subject</param>
        /// <returns>Newly added channel chat message</returns>
        public ITeamChatMessage AddBatch(string content, ChatMessageContentType contentType = ChatMessageContentType.Text, ITeamChatMessageAttachmentCollection attachments = null, string subject = null)
        {
            return AddBatchAsync(PnPContext.CurrentBatch, content, contentType, attachments, subject).GetAwaiter().GetResult();
        }

        /// <summary>
        /// Adds a new channel chat message
        /// </summary>
        /// <param name="batch">Batch the message is associated with</param>
        /// <param name="content">Content of the message</param>
        /// <param name="contentType">Message content type e.g. Text, Html</param>
        /// <param name="attachments">Attachments within the message</param>
        /// <param name="subject">Message subject</param>
        /// <returns>Newly added channel chat message</returns>
        public ITeamChatMessage AddBatch(Batch batch, string content, ChatMessageContentType contentType = ChatMessageContentType.Text, ITeamChatMessageAttachmentCollection attachments = null, string subject = null)
        {
            return AddBatchAsync(batch, content, contentType, attachments, subject).GetAwaiter().GetResult();
        }

    }
}
