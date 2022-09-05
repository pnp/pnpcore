using PnP.Core.Model.Security;

namespace PnP.Core.Model.Teams
{
    [GraphType]
    internal sealed class TeamConversationIdentity : BaseDataModel<ITeamConversationIdentity>, ITeamConversationIdentity
    {
        #region Properties
        public string DisplayName { get => GetValue<string>(); set => SetValue(value); }

        public string Id { get => GetValue<string>(); set => SetValue(value); }

        public TeamConversationIdentityType ConversationIdentityType { get => GetValue<TeamConversationIdentityType>(); set => SetValue(value); }
        #endregion
    }
}
