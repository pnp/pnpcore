using PnP.Core.Model.Teams;
using PnP.Core.QueryModel;
using PnP.Core.Services;
using System;
using System.Threading.Tasks;

namespace PnP.Core.Model.Me
{
    internal sealed class ChatMessageCollection : QueryableDataModelCollection<IChatMessage>, IChatMessageCollection
    {
        public ChatMessageCollection(PnPContext context, IDataModelParent parent, string memberName = null)
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
        public async Task<IChatMessage> AddAsync(ChatMessageOptions options)
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

            var newChannelChatMessage = CreateNewAndAdd() as ChatMessage;

            // Assign field values
            newChannelChatMessage.Body = new ChatMessageContent
            {
                PnPContext = newChannelChatMessage.PnPContext,
                Parent = newChannelChatMessage,
                Content = options.Content,
                ContentType = options.ContentType,
            };

            if (options.Attachments != null && options.Attachments.Count > 0)
            {
                var attachments = new ChatMessageAttachmentCollection();

                foreach (var optionAttachment in options.Attachments)
                {
                    attachments.Add(new ChatMessageAttachment()
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

                var hostedContents = new ChatMessageHostedContentCollection();

                foreach (var hostedContentOption in options.HostedContents)
                {
                    hostedContents.Add(new ChatMessageHostedContent()
                    {
                        Id = hostedContentOption.Id,
                        ContentBytes = hostedContentOption.ContentBytes,
                        ContentType = hostedContentOption.ContentType
                    });
                }

                newChannelChatMessage.HostedContents = hostedContents;
            }

            newChannelChatMessage.Subject = options.Subject;

            return await newChannelChatMessage.AddAsync().ConfigureAwait(false) as ChatMessage;
        }

        /// <summary>
        /// Adds a new channel chat message
        /// </summary>
        /// <param name="batch">Batch the message is associated with</param>
        /// <param name="options">Full chat message options</param>
        /// <returns>Newly added channel chat message</returns>
        public async Task<IChatMessage> AddBatchAsync(Batch batch, ChatMessageOptions options)
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

            var newChannelChatMessage = CreateNewAndAdd() as ChatMessage;

            // Assign field values
            newChannelChatMessage.Body = new ChatMessageContent
            {
                PnPContext = newChannelChatMessage.PnPContext,
                Parent = newChannelChatMessage,
                Content = options.Content,
                ContentType = options.ContentType,
            };

            if (options.Attachments != null && options.Attachments.Count > 0)
            {
                var attachments = new ChatMessageAttachmentCollection();

                foreach (var optionAttachment in options.Attachments)
                {
                    attachments.Add(new ChatMessageAttachment()
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

                var hostedContents = new ChatMessageHostedContentCollection();

                foreach (var hostedContentOption in options.HostedContents)
                {
                    hostedContents.Add(new ChatMessageHostedContent()
                    {
                        Id = hostedContentOption.Id,
                        ContentBytes = hostedContentOption.ContentBytes,
                        ContentType = hostedContentOption.ContentType
                    });
                }

                newChannelChatMessage.HostedContents = hostedContents;
            }

            newChannelChatMessage.Subject = options.Subject;

            return await newChannelChatMessage.AddBatchAsync(batch).ConfigureAwait(false) as ChatMessage;
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
        public async Task<IChatMessage> AddAsync(string content, ChatMessageContentType contentType = ChatMessageContentType.Text, string subject = null)
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
        public IChatMessage Add(string content, ChatMessageContentType contentType = ChatMessageContentType.Text, string subject = null)
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
        public async Task<IChatMessage> AddBatchAsync(string content, ChatMessageContentType contentType = ChatMessageContentType.Text, string subject = null)
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
        public IChatMessage AddBatch(string content, ChatMessageContentType contentType = ChatMessageContentType.Text, string subject = null)
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
        public IChatMessage AddBatch(Batch batch, string content, ChatMessageContentType contentType = ChatMessageContentType.Text, string subject = null)
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
        public async Task<IChatMessage> AddBatchAsync(Batch batch, string content, ChatMessageContentType contentType = ChatMessageContentType.Text, string subject = null)
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
        public IChatMessage Add(ChatMessageOptions options)
        {
            return AddAsync(options).GetAwaiter().GetResult();
        }

        /// <summary>
        /// Adds a new channel chat message
        /// </summary>
        /// <param name="batch">Batch the message is associated with</param>
        /// <param name="options">Full chat message options</param>
        /// <returns>Newly added channel chat message</returns>

        public IChatMessage AddBatch(Batch batch, ChatMessageOptions options)
        {
            return AddBatchAsync(batch, options).GetAwaiter().GetResult();
        }

        /// <summary>
        /// Adds a new channel chat message
        /// </summary>
        /// <param name="options">Full chat message options</param>
        /// <returns>Newly added channel chat message</returns>

        public async Task<IChatMessage> AddBatchAsync(ChatMessageOptions options)
        {
            return await AddBatchAsync(PnPContext.CurrentBatch, options).ConfigureAwait(false);
        }

        /// <summary>
        /// Adds a new channel chat message
        /// </summary>
        /// <param name="options">Full chat message options</param>
        /// <returns>Newly added channel chat message</returns>
        public IChatMessage AddBatch(ChatMessageOptions options)
        {
            return AddBatchAsync(PnPContext.CurrentBatch, options).GetAwaiter().GetResult();
        }

        #endregion
    }
}
