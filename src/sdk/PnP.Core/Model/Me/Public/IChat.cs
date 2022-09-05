using System;
using System.Linq;

namespace PnP.Core.Model.Me
{
    /// <summary>
    /// Defines a chat
    /// </summary>
    [ConcreteType(typeof(Chat))]
    public interface IChat : IDataModel<IChat>, IDataModelGet<IChat>, IDataModelLoad<IChat>, IDataModelUpdate, IQueryableDataModel
    {
        /// <summary>
        /// The unique Id of the chat conversation
        /// </summary>
        public string Id { get; }
        
        /// <summary>
        /// Topic of the chat
        /// </summary>
        public string Topic { get; }
        
        /// <summary>
        /// Teams Web url for the chat
        /// </summary>
        public string WebUrl { get; }

        /// <summary>
        /// Azure AD Tenant id for the tenant hosting the chat
        /// </summary>
        public Guid TenantId { get; }

        /// <summary>
        /// Created time of the chat
        /// </summary>
        public DateTime CreatedDateTime { get; }
        
        /// <summary>
        /// Last time the chat has been updated
        /// </summary>
        public DateTime LastUpdatedDateTime { get; }
        
        /// <summary>
        /// Type of the chat
        /// </summary>
        public string ChatType { get; }

        /// <summary>
        /// Members in this Team Chat
        /// Implements <see cref="IQueryable{T}"/>. <br />
        /// See <see href="https://pnp.github.io/pnpcore/using-the-sdk/basics-getdata.html#requesting-model-collections">Requesting model collections</see> 
        /// and <see href="https://pnp.github.io/pnpcore/using-the-sdk/basics-iqueryable.html">IQueryable performance considerations</see> to learn more.
        /// </summary>
        public IAadUserConversationMemberCollection Members { get; }

        /// <summary>
        /// Messages in this Team Chat
        /// Implements <see cref="IQueryable{T}"/>. <br />
        /// See <see href="https://pnp.github.io/pnpcore/using-the-sdk/basics-getdata.html#requesting-model-collections">Requesting model collections</see> 
        /// and <see href="https://pnp.github.io/pnpcore/using-the-sdk/basics-iqueryable.html">IQueryable performance considerations</see> to learn more.
        /// </summary>
        public IChatMessageCollection Messages { get; }
    }
}
