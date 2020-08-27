using System.Linq;
using System.Threading.Tasks;
using PnP.Core.Services;

namespace PnP.Core.Model.Teams
{
    /// <summary>
    /// A chat is a collection of chatMessages between one or more participants. Participants can be users or apps.
    /// </summary>
    public interface ITeamChatMessageCollection : IQueryable<ITeamChatMessage>, IDataModelCollection<ITeamChatMessage>, ISupportPaging<ITeamChatMessage>
    {
        /// <summary>
        /// Adds a new channel chat message
        /// </summary>
        /// <param name="body">Body of the chat message</param>
        /// <returns>Newly added channel chat message</returns>
        public Task<ITeamChatMessage> AddAsync(string body);

        /// <summary>
        /// Adds a new channel chat message
        /// </summary>
        /// <param name="body">Body of the chat message</param>
        /// <returns>Newly added channel chat message</returns>
        public ITeamChatMessage Add(string body);

        /// <summary>
        /// Adds a new channel chat message
        /// </summary>
        /// <param name="batch">Batch to use</param>
        /// <param name="body">Body of the chat message</param>
        /// <returns>Newly added channel chat message</returns>
        public Task<ITeamChatMessage> AddBatchAsync(Batch batch, string body);

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
        /// <param name="body">Body of the chat message</param>
        /// <returns>Newly added channel chat message</returns>
        public Task<ITeamChatMessage> AddBatchAsync(string body);

        /// <summary>
        /// Adds a new channel chat message
        /// </summary>
        /// <param name="body">Body of the chat message</param>
        /// <returns>Newly added channel chat message</returns>
        public ITeamChatMessage AddBatch(string body);
    }
}
