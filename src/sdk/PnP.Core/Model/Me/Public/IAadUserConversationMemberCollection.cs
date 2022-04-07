using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PnP.Core.Model.Me
{
    /// <summary>
    /// 
    /// </summary>
    [ConcreteType(typeof(AadUserConversationMemberCollection))]
    public interface IAadUserConversationMemberCollection : IQueryable<IAadUserConversationMember>, IAsyncEnumerable<IAadUserConversationMember>, IDataModelCollection<IAadUserConversationMember>, IDataModelCollectionLoad<IAadUserConversationMember>, ISupportModules<IAadUserConversationMemberCollection>
    {
    }
}
