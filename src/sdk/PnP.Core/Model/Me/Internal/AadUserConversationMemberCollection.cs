using PnP.Core.QueryModel;
using PnP.Core.Services;

namespace PnP.Core.Model.Me
{
    internal sealed class AadUserConversationMemberCollection : QueryableDataModelCollection<IAadUserConversationMember>, IAadUserConversationMemberCollection
    {
        public AadUserConversationMemberCollection(PnPContext context, IDataModelParent parent, string memberName = null)
            : base(context, parent, memberName)
        {
            PnPContext = context;
            Parent = parent;
        }
    }
}
