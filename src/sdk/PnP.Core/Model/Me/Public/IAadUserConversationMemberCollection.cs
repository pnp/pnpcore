using System.Collections.Generic;
using System.Linq;

namespace PnP.Core.Model.Me
{
    /// <summary>
    /// Collection of users participating in a chat
    /// </summary>
    [ConcreteType(typeof(AadUserConversationMemberCollection))]
    public interface IAadUserConversationMemberCollection : IQueryable<IAadUserConversationMember>, IAsyncEnumerable<IAadUserConversationMember>, IDataModelCollection<IAadUserConversationMember>, IDataModelCollectionLoad<IAadUserConversationMember>, ISupportModules<IAadUserConversationMemberCollection>
    {
    }
}
