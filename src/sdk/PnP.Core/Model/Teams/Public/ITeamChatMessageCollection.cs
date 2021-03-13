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

        #region Basic Messages

        /// <summary>
        /// Adds a new channel chat message
        /// </summary>
        /// <param name="content">Content of the message</param>
        /// <param name="contentType">Message content type e.g. Text, Html</param>
        /// <param name="subject">Message Subject</param>
        /// <returns></returns>
        public Task<ITeamChatMessage> AddAsync(string content, ChatMessageContentType contentType = ChatMessageContentType.Text, string subject = null);

        /// <summary>
        /// Adds a new channel chat
        /// </summary>
        /// <param name="content">Content of the message</param>
        /// <param name="contentType">Message content type e.g. Text, Html</param>
        /// /// <param name="subject">Message Subject</param>
        /// <returns></returns>
        public ITeamChatMessage Add(string content, ChatMessageContentType contentType = ChatMessageContentType.Text, string subject = null);

        /// <summary>
        /// Adds a new channel chat message
        /// </summary>
        /// <param name="batch">Batch the message is associated with</param>
        /// <param name="content">Content of the message</param>
        /// <param name="contentType">Message content type e.g. Text, Html</param>
        /// <param name="subject">Message Subject</param>
        /// <returns>Newly added channel chat message</returns>
        public Task<ITeamChatMessage> AddBatchAsync(Batch batch, string content, ChatMessageContentType contentType = ChatMessageContentType.Text, string subject = null);

        /// <summary>
        /// Adds a new channel chat message
        /// </summary>
        /// <param name="batch">Batch the message is associated with</param>
        /// <param name="content">Content of the message</param>
        /// <param name="contentType">Message content type e.g. Text, Html</param>
        /// <param name="subject">Message Subject</param>
        /// <returns>Newly added channel chat message</returns>
        public ITeamChatMessage AddBatch(Batch batch, string content, ChatMessageContentType contentType = ChatMessageContentType.Text, string subject = null);

        /// <summary>
        /// Adds a new channel chat message
        /// </summary>
        /// <param name="content">Content of the message</param>
        /// <param name="contentType">Message content type e.g. Text, Html</param>
        /// <param name="subject">Message Subject</param>
        /// <returns>Newly added channel chat message</returns>
        public Task<ITeamChatMessage> AddBatchAsync(string content, ChatMessageContentType contentType = ChatMessageContentType.Text, string subject = null);

        /// <summary>
        /// Adds a new channel chat message
        /// </summary>
        /// <param name="content">Content of the message</param>
        /// <param name="contentType">Message content type e.g. Text, Html</param>
        /// <param name="subject">Message Subject</param>
        /// <returns>Newly added channel chat message</returns>
        public ITeamChatMessage AddBatch(string content, ChatMessageContentType contentType = ChatMessageContentType.Text, string subject = null);

        #endregion

        // With options

        #region Advanced Messages

        /// <summary>
        /// Adds a new channel chat message
        /// </summary>
        /// <param name="options">Full chat message options</param>
        /// <returns></returns>
        public Task<ITeamChatMessage> AddAsync(ChatMessageOptions options);

        /// <summary>
        /// Adds a new channel chat
        /// </summary>
        /// <param name="options">Full chat message options</param>
        /// <returns></returns>
        public ITeamChatMessage Add(ChatMessageOptions options);

        /// <summary>
        /// Adds a new channel chat message
        /// </summary>
        /// <param name="batch">Batch the message is associated with</param>
        /// <param name="options">Full chat message options</param>
        /// <returns>Newly added channel chat message</returns>
        public Task<ITeamChatMessage> AddBatchAsync(Batch batch, ChatMessageOptions options);

        /// <summary>
        /// Adds a new channel chat message
        /// </summary>
        /// <param name="batch">Batch the message is associated with</param>
        /// <param name="options">Full chat message options</param>
        /// <returns>Newly added channel chat message</returns>
        public ITeamChatMessage AddBatch(Batch batch, ChatMessageOptions options);

        /// <summary>
        /// Adds a new channel chat message
        /// </summary>
        /// <param name="options">Full chat message options</param>
        /// <returns>Newly added channel chat message</returns>
        public Task<ITeamChatMessage> AddBatchAsync(ChatMessageOptions options);

        /// <summary>
        /// Adds a new channel chat message
        /// </summary>
        /// <param name="options">Full chat message options</param>
        /// <returns>Newly added channel chat message</returns>
        public ITeamChatMessage AddBatch(ChatMessageOptions options);

        #endregion

    }
}
