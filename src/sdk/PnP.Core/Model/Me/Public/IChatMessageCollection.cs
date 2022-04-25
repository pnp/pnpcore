using PnP.Core.Model.Teams;
using PnP.Core.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PnP.Core.Model.Me
{
    /// <summary>
    /// 
    /// </summary>
    /// <summary>
    /// A chat is a collection of chatMessages between one or more participants. Participants can be users or apps.
    /// </summary>
    [ConcreteType(typeof(ChatMessageCollection))]
    public interface IChatMessageCollection : IQueryable<IChatMessage>, IAsyncEnumerable<IChatMessage>, IDataModelCollectionLoad<IChatMessage>, IDataModelCollection<IChatMessage>, ISupportModules<IChatMessageCollection>
    {

        #region Basic Messages

        /// <summary>
        /// Adds a new channel chat message
        /// </summary>
        /// <param name="content">Content of the message</param>
        /// <param name="contentType">Message content type e.g. Text, Html</param>
        /// <param name="subject">Message Subject</param>
        /// <returns></returns>
        public Task<IChatMessage> AddAsync(string content, ChatMessageContentType contentType = ChatMessageContentType.Text, string subject = null);

        /// <summary>
        /// Adds a new channel chat
        /// </summary>
        /// <param name="content">Content of the message</param>
        /// <param name="contentType">Message content type e.g. Text, Html</param>
        /// /// <param name="subject">Message Subject</param>
        /// <returns></returns>
        public IChatMessage Add(string content, ChatMessageContentType contentType = ChatMessageContentType.Text, string subject = null);

        /// <summary>
        /// Adds a new channel chat message
        /// </summary>
        /// <param name="batch">Batch the message is associated with</param>
        /// <param name="content">Content of the message</param>
        /// <param name="contentType">Message content type e.g. Text, Html</param>
        /// <param name="subject">Message Subject</param>
        /// <returns>Newly added channel chat message</returns>
        public Task<IChatMessage> AddBatchAsync(Batch batch, string content, ChatMessageContentType contentType = ChatMessageContentType.Text, string subject = null);

        /// <summary>
        /// Adds a new channel chat message
        /// </summary>
        /// <param name="batch">Batch the message is associated with</param>
        /// <param name="content">Content of the message</param>
        /// <param name="contentType">Message content type e.g. Text, Html</param>
        /// <param name="subject">Message Subject</param>
        /// <returns>Newly added channel chat message</returns>
        public IChatMessage AddBatch(Batch batch, string content, ChatMessageContentType contentType = ChatMessageContentType.Text, string subject = null);

        /// <summary>
        /// Adds a new channel chat message
        /// </summary>
        /// <param name="content">Content of the message</param>
        /// <param name="contentType">Message content type e.g. Text, Html</param>
        /// <param name="subject">Message Subject</param>
        /// <returns>Newly added channel chat message</returns>
        public Task<IChatMessage> AddBatchAsync(string content, ChatMessageContentType contentType = ChatMessageContentType.Text, string subject = null);

        /// <summary>
        /// Adds a new channel chat message
        /// </summary>
        /// <param name="content">Content of the message</param>
        /// <param name="contentType">Message content type e.g. Text, Html</param>
        /// <param name="subject">Message Subject</param>
        /// <returns>Newly added channel chat message</returns>
        public IChatMessage AddBatch(string content, ChatMessageContentType contentType = ChatMessageContentType.Text, string subject = null);

        #endregion

        // With options

        #region Advanced Messages

        /// <summary>
        /// Adds a new channel chat message
        /// </summary>
        /// <param name="options">Full chat message options</param>
        /// <returns></returns>
        public Task<IChatMessage> AddAsync(ChatMessageOptions options);

        /// <summary>
        /// Adds a new channel chat
        /// </summary>
        /// <param name="options">Full chat message options</param>
        /// <returns></returns>
        public IChatMessage Add(ChatMessageOptions options);

        /// <summary>
        /// Adds a new channel chat message
        /// </summary>
        /// <param name="batch">Batch the message is associated with</param>
        /// <param name="options">Full chat message options</param>
        /// <returns>Newly added channel chat message</returns>
        public Task<IChatMessage> AddBatchAsync(Batch batch, ChatMessageOptions options);

        /// <summary>
        /// Adds a new channel chat message
        /// </summary>
        /// <param name="batch">Batch the message is associated with</param>
        /// <param name="options">Full chat message options</param>
        /// <returns>Newly added channel chat message</returns>
        public IChatMessage AddBatch(Batch batch, ChatMessageOptions options);

        /// <summary>
        /// Adds a new channel chat message
        /// </summary>
        /// <param name="options">Full chat message options</param>
        /// <returns>Newly added channel chat message</returns>
        public Task<IChatMessage> AddBatchAsync(ChatMessageOptions options);

        /// <summary>
        /// Adds a new channel chat message
        /// </summary>
        /// <param name="options">Full chat message options</param>
        /// <returns>Newly added channel chat message</returns>
        public IChatMessage AddBatch(ChatMessageOptions options);

        #endregion

    }
}
