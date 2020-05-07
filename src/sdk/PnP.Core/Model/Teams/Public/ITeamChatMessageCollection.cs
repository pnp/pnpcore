using System.Threading.Tasks;

namespace PnP.Core.Model.Teams
{
    /// <summary>
    /// A chat is a collection of chatMessages between one or more participants. Participants can be users or apps.
    /// </summary>
    public interface ITeamChatMessageCollection : IDataModelCollection<ITeamChatMessage>, ISupportPaging
    {
        /// <summary>
        /// Adds a new channel chat message
        /// </summary>
        /// <param name="name">Body of the chat message</param>
        /// <returns>Newly added channel chat message</returns>
        public Task<ITeamChatMessage> AddAsync(string body);

        /// <summary>
        /// Adds a new channel chat message
        /// </summary>
        /// <param name="batch">Batch to use</param>
        /// <param name="name">Body of the chat message</param>
        /// <returns>Newly added channel chat message</returns>
        public ITeamChatMessage Add(Batch batch, string body);

        /// <summary>
        /// Adds a new channel chat message
        /// </summary>
        /// <param name="name">Body of the chat message</param>
        /// <returns>Newly added channel chat message</returns>
        public ITeamChatMessage Add(string body);
    }
}
