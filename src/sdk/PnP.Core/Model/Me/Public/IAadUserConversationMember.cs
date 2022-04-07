using PnP.Core.Model.Security;
using System;
using System.Collections.Generic;
using System.Text;

namespace PnP.Core.Model.Me
{
    /// <summary>
    /// 
    /// </summary>
    [ConcreteType(typeof(AadUserConversationMember))]
    public interface IAadUserConversationMember : IDataModel<IAadUserConversationMember>, IDataModelGet<IAadUserConversationMember>, IDataModelLoad<IAadUserConversationMember>, IDataModelUpdate
    {

        /// <summary>
        /// 
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public List<string> Roles { get; set; }
        
        /// <summary>
        /// 
        /// </summary>
        public string UserId { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string TenantId { get; }
        
        /// <summary>
        /// 
        /// </summary>
        public string Email { get; }

        /// <summary>
        /// 
        /// </summary>
        public string DisplayName { get; }

        /// <summary>
        /// 
        /// </summary>
        public DateTime VisibleHistoryStartDateTime { get; }
    }
}
