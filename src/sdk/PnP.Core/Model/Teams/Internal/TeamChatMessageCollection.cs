using System;
using System.Threading.Tasks;
using PnP.Core.QueryModel;
using PnP.Core.Services;

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
                ContentType = ChatMessageContentType.Text,
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
        /// <param name="batch">Batch to use</param>
        /// <param name="body">Body of the chat message</param>
        /// <returns>Newly added channel chat message</returns>
        public async Task<ITeamChatMessage> AddBatchAsync(Batch batch, string body)
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
                ContentType = ChatMessageContentType.Text
            };

            return await newChannelChatMessage.AddBatchAsync(batch).ConfigureAwait(false) as TeamChatMessage;
        }

        /// <summary>
        /// Adds a new channel chat message
        /// </summary>
        /// <param name="batch">Batch to use</param>
        /// <param name="body">Body of the chat message</param>
        /// <returns>Newly added channel chat message</returns>
        public ITeamChatMessage AddBatch(Batch batch, string body)
        {
            return AddBatchAsync(batch, body).GetAwaiter().GetResult();
        }

        /// <summary>
        /// Adds a new channel chat message
        /// </summary>
        /// <param name="body">Body of the chat message</param>
        /// <returns>Newly added channel chat message</returns>
        public async Task<ITeamChatMessage> AddBatchAsync(string body)
        {
            return await AddBatchAsync(PnPContext.CurrentBatch, body).ConfigureAwait(false);
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

    }
}
