using PnP.Core.Services;
using System.Linq;
using System.Threading.Tasks;

namespace PnP.Core.Model.Teams
{
    /// <summary>
    /// A chat is a collection of chatMessages between one or more participants. Participants can be users or apps.
    /// </summary>
    [ConcreteType(typeof(TeamChatMessageCollection))]
    public interface ITeamChatMessageCollection : IQueryable<ITeamChatMessage>, IDataModelCollection<ITeamChatMessage>, ISupportPaging<ITeamChatMessage>
    {
        /// <summary>
        /// Adds a new channel chat message
        /// </summary>
        /// <param name="body">Body of the chat message</param>
        /// <returns>Newly added channel chat message</returns>
        public Task<ITeamChatMessage> AddAsync(string body);


        /// <summary>
        /// Adds a new channel chat message with more supported types
        /// </summary>
        /// <param name="body"></param>
        /// <param name="contentType"></param>
        /// <param name="attachments"></param>
        /// <returns></returns>
        public Task<ITeamChatMessage> AddAsync(string body, ChatMessageContentType contentType, ITeamChatMessageAttachmentCollection attachments);

        /// <summary>
        /// Adds a new channel chat message
        /// </summary>
        /// <param name="body">Body of the chat message</param>
        /// <returns>Newly added channel chat message</returns>
        public ITeamChatMessage Add(string body);

        /// <summary>
        /// Adds a new channel chat
        /// </summary>
        /// <param name="body"></param>
        /// <param name="contentType"></param>
        /// <param name="attachments"></param>
        /// <returns></returns>
        public ITeamChatMessage Add(string body, ChatMessageContentType contentType, ITeamChatMessageAttachmentCollection attachments);

        /// <summary>
        /// Adds a new channel chat message
        /// </summary>
        /// <param name="batch">Batch to use</param>
        /// <param name="body">Body of the chat message</param>
        /// <returns>Newly added channel chat message</returns>
        public Task<ITeamChatMessage> AddBatchAsync(Batch batch, string body);


        /// <summary>
        /// Adds new channel chat message
        /// </summary>
        /// <param name="batch"></param>
        /// <param name="body"></param>
        /// <param name="contentType"></param>
        /// <returns></returns>
        public Task<ITeamChatMessage> AddBatchAsync(Batch batch, string body, ChatMessageContentType contentType);

        /// <summary>
        /// Adds a new channel chat message
        /// </summary>
        /// <param name="batch">Batch to use</param>
        /// <param name="body">Body of the chat message</param>
        /// <returns>Newly added channel chat message</returns>
        public ITeamChatMessage AddBatch(Batch batch, string body);

        /// <summary>
        /// Adds a new channel chat message
        /// </summary>
        /// <param name="batch"></param>
        /// <param name="body"></param>
        /// <param name="contentType"></param>
        /// <returns></returns>
        public ITeamChatMessage AddBatch(Batch batch, string body, ChatMessageContentType contentType);


        /// <summary>
        /// Adds a new channel chat message
        /// </summary>
        /// <param name="body">Body of the chat message</param>
        /// <returns>Newly added channel chat message</returns>
        public Task<ITeamChatMessage> AddBatchAsync(string body);

        /// <summary>
        /// Adds a new channel chat message
        /// </summary>
        /// <param name="body"></param>
        /// <param name="contentType"></param>
        /// <returns></returns>
        public Task<ITeamChatMessage> AddBatchAsync(string body, ChatMessageContentType contentType);

        /// <summary>
        /// Adds a new channel chat message
        /// </summary>
        /// <param name="body">Body of the chat message</param>
        /// <returns>Newly added channel chat message</returns>
        public ITeamChatMessage AddBatch(string body);

        /// <summary>
        /// Adds a new channel chat message
        /// </summary>
        /// <param name="body"></param>
        /// <param name="contentType"></param>
        /// <returns></returns>
        public ITeamChatMessage AddBatch(string body, ChatMessageContentType contentType);
    }
}
