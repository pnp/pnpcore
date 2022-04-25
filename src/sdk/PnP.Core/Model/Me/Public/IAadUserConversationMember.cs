using System;
using System.Collections.Generic;

namespace PnP.Core.Model.Me
{
    /// <summary>
    /// User participating in a conversation (chat)
    /// </summary>
    [ConcreteType(typeof(AadUserConversationMember))]
    public interface IAadUserConversationMember : IDataModel<IAadUserConversationMember>, IDataModelGet<IAadUserConversationMember>, IDataModelLoad<IAadUserConversationMember>, IDataModelUpdate
    {

        /// <summary>
        /// Id of the user in the chat
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Roles the user has
        /// </summary>
        public List<string> Roles { get; set; }
        
        /// <summary>
        /// The actual user id
        /// </summary>
        public string UserId { get; set; }

        /// <summary>
        /// The Azuire tenant id of the tenant the user is in
        /// </summary>
        public string TenantId { get; }
        
        /// <summary>
        /// Email of the user
        /// </summary>
        public string Email { get; }

        /// <summary>
        /// Display name of the user
        /// </summary>
        public string DisplayName { get; }

        /// <summary>
        /// User can see chat history from this date
        /// </summary>
        public DateTime VisibleHistoryStartDateTime { get; }
    }
}
