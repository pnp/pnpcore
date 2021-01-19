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
        /// Adds a new channel chat message
        /// </summary>
        /// <param name="body">Body of the chat message</param>
        /// <returns>Newly added channel chat message</returns>
        public async Task<ITeamChatMessage> AddAsync(string body)
        {
            return await AddAsync(body, ChatMessageContentType.Text).ConfigureAwait(false);
        }

        /// <summary>
        /// Adds new channel chat message with support for content types
        /// </summary>
        /// <param name="body"></param>
        /// <param name="contentType"></param>
        /// <returns></returns>
        public async Task<ITeamChatMessage> AddAsync(string body, ChatMessageContentType contentType)
        {
            if (string.IsNullOrEmpty(body))
            {
                throw new ArgumentNullException(nameof(body));
            }

            var newChannelChatMessage = CreateNewAndAdd() as TeamChatMessage;

            // Assign field values
            newChannelChatMessage.Body = new TeamChatMessageContent
            {
                PnPContext = newChannelChatMessage.PnPContext,
                Parent = newChannelChatMessage,
                Content = body,
                ContentType = contentType,
            };


            return await newChannelChatMessage.AddAsync().ConfigureAwait(false) as TeamChatMessage;
        }

        /// <summary>
        /// Adds a new channel chat message
        /// </summary>
        /// <param name="body">Body of the chat message</param>
        /// <returns>Newly added channel chat message</returns>
        public ITeamChatMessage Add(string body)
        {
            return AddAsync(body).GetAwaiter().GetResult();
        }

        /// <summary>
        /// Adds a new channel chat message
        /// </summary>
        /// <param name="body"></param>
        /// <param name="contentType"></param>
        /// <returns></returns>
        public ITeamChatMessage Add(string body, ChatMessageContentType contentType)
        {
            return AddAsync(body, contentType).GetAwaiter().GetResult();
        }

        /// <summary>
        /// Adds a new channel chat message
        /// </summary>
        /// <param name="batch">Batch to use</param>
        /// <param name="body">Body of the chat message</param>
        /// <param name="contentType">content type of the message</param>
        /// <returns>Newly added channel chat message</returns>
        public async Task<ITeamChatMessage> AddBatchAsync(Batch batch, string body, ChatMessageContentType contentType)
        {
            if (string.IsNullOrEmpty(body))
            {
                throw new ArgumentNullException(nameof(body));
            }

            var newChannelChatMessage = CreateNewAndAdd() as TeamChatMessage;

            // Assign field values
            newChannelChatMessage.Body = new TeamChatMessageContent
            {
                PnPContext = newChannelChatMessage.PnPContext,
                Parent = newChannelChatMessage,
                Content = body,
                ContentType = contentType
            };

            return await newChannelChatMessage.AddBatchAsync(batch).ConfigureAwait(false) as TeamChatMessage;
        }

        /// <summary>
        /// Adds a new channel chat message 
        /// </summary>
        /// <param name="batch"></param>
        /// <param name="body"></param>
        /// <returns></returns>
        public async Task<ITeamChatMessage> AddBatchAsync(Batch batch, string body)
        {
            return await AddBatchAsync(batch, body, ChatMessageContentType.Text).ConfigureAwait(false);
        }

        /// <summary>
        /// Adds a new channel chat message
        /// </summary>
        /// <param name="batch">Batch to use</param>
        /// <param name="body">Body of the chat message</param>
        /// <returns>Newly added channel chat message</returns>
        public ITeamChatMessage AddBatch(Batch batch, string body)
        {
            return AddBatchAsync(batch, body, ChatMessageContentType.Text).GetAwaiter().GetResult();
        }

        /// <summary>
        /// Adds a new channel chat message
        /// </summary>
        /// <param name="batch"></param>
        /// <param name="body"></param>
        /// <param name="contentType"></param>
        /// <returns></returns>
        public ITeamChatMessage AddBatch(Batch batch, string body, ChatMessageContentType contentType)
        {
            return AddBatchAsync(batch, body, contentType).GetAwaiter().GetResult();
        }

        /// <summary>
        /// Adds a new channel chat message
        /// </summary>
        /// <param name="body">Body of the chat message</param>
        /// <returns>Newly added channel chat message</returns>
        public async Task<ITeamChatMessage> AddBatchAsync(string body)
        {
            return await AddBatchAsync(PnPContext.CurrentBatch, body, ChatMessageContentType.Text).ConfigureAwait(false);
        }

        /// <summary>
        /// Adds a new channel chat message
        /// </summary>
        /// <param name="body"></param>
        /// <param name="contentType"></param>
        /// <returns></returns>
        public async Task<ITeamChatMessage> AddBatchAsync(string body, ChatMessageContentType contentType)
        {
            return await AddBatchAsync(PnPContext.CurrentBatch, body, contentType).ConfigureAwait(false);
        }

        /// <summary>
        /// Adds a new channel chat message
        /// </summary>
        /// <param name="body">Body of the chat message</param>
        /// <returns>Newly added channel chat message</returns>
        public ITeamChatMessage AddBatch(string body)
        {
            return AddBatchAsync(body).GetAwaiter().GetResult();
        }

        /// <summary>
        /// Adds a new channel chat message
        /// </summary>
        /// <param name="body"></param>
        /// <param name="contentType"></param>
        /// <returns></returns>
        public ITeamChatMessage AddBatch(string body, ChatMessageContentType contentType)
        {
            return AddBatchAsync(body, contentType).GetAwaiter().GetResult();
        }

    }
}
