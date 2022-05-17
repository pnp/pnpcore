using PnP.Core.Services;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PnP.Core.Model.Teams
{
    /// <summary>
    /// A collection of the replies on a chat
    /// </summary>
    [ConcreteType(typeof(TeamChatMessageReplyCollection))]
    public interface ITeamChatMessageReplyCollection : IQueryable<ITeamChatMessageReply>, IAsyncEnumerable<ITeamChatMessageReply>, IDataModelCollectionLoad<ITeamChatMessageReply>, IDataModelCollection<ITeamChatMessageReply>, ISupportModules<ITeamChatMessageReplyCollection>
    {
    }
}
