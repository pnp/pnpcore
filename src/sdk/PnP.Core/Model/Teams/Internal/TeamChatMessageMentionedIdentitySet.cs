using PnP.Core.Model.Security;

namespace PnP.Core.Model.Teams
{
    [GraphType]
    internal sealed class TeamChatMessageMentionedIdentitySet : BaseDataModel<ITeamChatMessageMentionedIdentitySet>, ITeamChatMessageMentionedIdentitySet
    {
        #region Properties

        public IIdentity Application { get => GetModelValue<IIdentity>(); set => SetModelValue(value); }

        public IIdentity User { get => GetModelValue<IIdentity>(); set => SetModelValue(value); }

        public ITeamTagIdentity Tag { get => GetModelValue<ITeamTagIdentity>(); set => SetModelValue(value); }

        public ITeamConversationIdentity Conversation { get => GetModelValue<ITeamConversationIdentity>(); set => SetModelValue(value); }
        #endregion
    }
}
