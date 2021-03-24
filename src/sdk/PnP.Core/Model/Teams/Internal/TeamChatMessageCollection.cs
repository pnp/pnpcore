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

        #region Main Methods

        /// <summary>
        /// Adds new channel chat message with support for content types
        /// </summary>
        /// <param name="options">Full chat message options</param>
        /// <returns></returns>
        public async Task<ITeamChatMessage> AddAsync(ChatMessageOptions options)
        {
            if(options == default)
            {
                throw new ArgumentNullException(nameof(options));
            }

            //Minimum for a message
            if (string.IsNullOrEmpty(options.Content))
            {
                throw new ArgumentNullException(nameof(options), "parameter must include message content");
            }

            var newChannelChatMessage = CreateNewAndAdd() as TeamChatMessage;

            // Assign field values
            newChannelChatMessage.Body = new TeamChatMessageContent
            {
                PnPContext = newChannelChatMessage.PnPContext,
                Parent = newChannelChatMessage,
                Content = options.Content,
                ContentType = options.ContentType,
            };

            if(options.Attachments != null && options.Attachments.Count > 0)
            {
                var attachments = new TeamChatMessageAttachmentCollection();

                foreach(var optionAttachment in options.Attachments)
                {
                    attachments.Add(new TeamChatMessageAttachment()
                    {
                        Id = optionAttachment.Id,
                        Content = optionAttachment.Content,
                        ContentType = optionAttachment.ContentType,
                        Name = optionAttachment.Name,
                        ContentUrl = optionAttachment.ContentUrl,
                        ThumbnailUrl = optionAttachment.ThumbnailUrl
                    });
                }

                newChannelChatMessage.Attachments = attachments;
            }

            if (options.HostedContents != null && options.HostedContents.Count > 0)
            {

                var hostedContents = new TeamChatMessageHostedContentCollection();

                foreach (var hostedContentOption in options.HostedContents)
                {
                    hostedContents.Add(new TeamChatMessageHostedContent()
                    {
                        Id = hostedContentOption.Id,
                        ContentBytes = hostedContentOption.ContentBytes,
                        ContentType = hostedContentOption.ContentType
                    });
                }

                newChannelChatMessage.HostedContents = hostedContents;
            }

            newChannelChatMessage.Subject = options.Subject;

            return await newChannelChatMessage.AddAsync().ConfigureAwait(false) as TeamChatMessage;
        }

        /// <summary>
        /// Adds a new channel chat message
        /// </summary>
        /// <param name="batch">Batch the message is associated with</param>
        /// <param name="options">Full chat message options</param>
        /// <returns>Newly added channel chat message</returns>
        public async Task<ITeamChatMessage> AddBatchAsync(Batch batch, ChatMessageOptions options)
        {
            if (options == default)
            {
                throw new ArgumentNullException(nameof(options));
            }

            //Minimum for a message
            if (string.IsNullOrEmpty(options.Content))
            {
                throw new ArgumentNullException(nameof(options), "parameter must include message content");
            }

            var newChannelChatMessage = CreateNewAndAdd() as TeamChatMessage;

            // Assign field values
            newChannelChatMessage.Body = new TeamChatMessageContent
            {
                PnPContext = newChannelChatMessage.PnPContext,
                Parent = newChannelChatMessage,
                Content = options.Content,
                ContentType = options.ContentType,
            };

            if (options.Attachments != null && options.Attachments.Count > 0)
            {
                var attachments = new TeamChatMessageAttachmentCollection();

                foreach (var optionAttachment in options.Attachments)
                {
                    attachments.Add(new TeamChatMessageAttachment()
                    {
                        Id = optionAttachment.Id,
                        Content = optionAttachment.Content,
                        ContentType = optionAttachment.ContentType,
                        Name = optionAttachment.Name,
                        ContentUrl = optionAttachment.ContentUrl,
                        ThumbnailUrl = optionAttachment.ThumbnailUrl
                    });
                }

                newChannelChatMessage.Attachments = attachments;
            }

            if (options.HostedContents != null && options.HostedContents.Count > 0)
            {

                var hostedContents = new TeamChatMessageHostedContentCollection();

                foreach (var hostedContentOption in options.HostedContents)
                {
                    hostedContents.Add(new TeamChatMessageHostedContent()
                    {
                        Id = hostedContentOption.Id,
                        ContentBytes = hostedContentOption.ContentBytes,
                        ContentType = hostedContentOption.ContentType
                    });
                }

                newChannelChatMessage.HostedContents = hostedContents;
            }

            newChannelChatMessage.Subject = options.Subject;

            return await newChannelChatMessage.AddBatchAsync(batch).ConfigureAwait(false) as TeamChatMessage;
        }

        #endregion


        #region Basic Messages Overloads

        /// <summary>
        /// Adds a new channel chat message 
        /// </summary>
        /// <param name="content"></param>
        /// <param name="contentType"></param>
        /// <param name="subject">Message Subject</param>
        /// <returns></returns>
        public async Task<ITeamChatMessage> AddAsync(string content, ChatMessageContentType contentType = ChatMessageContentType.Text, string subject = null)
        {
            return await AddAsync(new ChatMessageOptions() { Content = content, ContentType = contentType, Subject = subject }).ConfigureAwait(false);
        }

        /// <summary>
        /// Adds a new channel chat message
        /// </summary>
        /// <param name="content">Content of the message</param>
        /// <param name="contentType">Message content type e.g. Text, Html</param>
        /// <param name="subject">Message Subject</param>
        /// <returns>Newly added channel chat message</returns>
        public ITeamChatMessage Add(string content, ChatMessageContentType contentType = ChatMessageContentType.Text, string subject = null)
        {
            return AddAsync(new ChatMessageOptions() { Content = content, ContentType = contentType, Subject = subject }).GetAwaiter().GetResult();
        }

        /// <summary>
        /// Adds a new channel chat message
        /// </summary>
        /// <param name="content">Content of the message</param>
        /// <param name="contentType">Message content type e.g. Text, Html</param>
        /// <param name="subject">Message Subject</param>
        /// <returns>Newly added channel chat message</returns>
        public async Task<ITeamChatMessage> AddBatchAsync(string content, ChatMessageContentType contentType = ChatMessageContentType.Text, string subject = null)
        {
            return await AddBatchAsync(PnPContext.CurrentBatch, new ChatMessageOptions() { Content = content, ContentType = contentType, Subject = subject }).ConfigureAwait(false);
        }

        /// <summary>
        /// Adds a new channel chat message
        /// </summary>
        /// <param name="content">Content of the message</param>
        /// <param name="contentType">Message content type e.g. Text, Html</param>
        /// <param name="subject">Message Subject</param>
        /// <returns>Newly added channel chat message</returns>
        public ITeamChatMessage AddBatch(string content, ChatMessageContentType contentType = ChatMessageContentType.Text, string subject = null)
        {
            return AddBatchAsync(PnPContext.CurrentBatch, new ChatMessageOptions() { Content = content, ContentType = contentType, Subject = subject }).GetAwaiter().GetResult();
        }

        /// <summary>
        /// Adds a new channel chat message
        /// </summary>
        /// <param name="batch">Batch the message is associated with</param>
        /// <param name="content">Content of the message</param>
        /// <param name="contentType">Message content type e.g. Text, Html</param>
        /// <param name="subject">Message Subject</param>
        /// <returns>Newly added channel chat message</returns>
        public ITeamChatMessage AddBatch(Batch batch, string content, ChatMessageContentType contentType = ChatMessageContentType.Text, string subject = null)
        {
            return AddBatchAsync(batch, new ChatMessageOptions() { Content = content, ContentType = contentType, Subject = subject }).GetAwaiter().GetResult();
        }


        /// <summary>
        /// Adds a new channel chat message
        /// </summary>
        /// <param name="batch">Batch the message is associated with</param>
        /// <param name="content">Content of the message</param>
        /// <param name="contentType">Message content type e.g. Text, Html</param>
        /// <param name="subject">Message Subject</param>
        /// <returns></returns>
        public async Task<ITeamChatMessage> AddBatchAsync(Batch batch, string content, ChatMessageContentType contentType = ChatMessageContentType.Text, string subject = null)
        {
            return await AddBatchAsync(batch, new ChatMessageOptions() { Content = content, ContentType = contentType, Subject = subject }).ConfigureAwait(false);
        }

        #endregion


        #region Advanced Message Overloads

        /// <summary>
        /// Adds a new channel chat message
        /// </summary>
        /// <param name="options">Full chat message options</param>
        /// <returns>Newly added channel chat message</returns>
        public ITeamChatMessage Add(ChatMessageOptions options)
        {
            return AddAsync(options).GetAwaiter().GetResult();
        }

        /// <summary>
        /// Adds a new channel chat message
        /// </summary>
        /// <param name="batch">Batch the message is associated with</param>
        /// <param name="options">Full chat message options</param>
        /// <returns>Newly added channel chat message</returns>

        public ITeamChatMessage AddBatch(Batch batch, ChatMessageOptions options)
        {
            return AddBatchAsync(batch, options).GetAwaiter().GetResult();
        }

        /// <summary>
        /// Adds a new channel chat message
        /// </summary>
        /// <param name="options">Full chat message options</param>
        /// <returns>Newly added channel chat message</returns>

        public async Task<ITeamChatMessage> AddBatchAsync(ChatMessageOptions options)
        {
            return await AddBatchAsync(PnPContext.CurrentBatch, options).ConfigureAwait(false);
        }

        /// <summary>
        /// Adds a new channel chat message
        /// </summary>
        /// <param name="options">Full chat message options</param>
        /// <returns>Newly added channel chat message</returns>
        public ITeamChatMessage AddBatch(ChatMessageOptions options)
        {
            return AddBatchAsync(PnPContext.CurrentBatch, options).GetAwaiter().GetResult();
        }

        #endregion
    }
}
