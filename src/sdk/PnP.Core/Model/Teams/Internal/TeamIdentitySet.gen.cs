using PnP.Core.Model.Security;

namespace PnP.Core.Model.Teams
{
    [GraphType]
    internal partial class TeamIdentitySet : BaseComplexType<ITeamIdentitySet>, ITeamIdentitySet
    {
        public IIdentity Application { get => GetValue<IIdentity>(); set => SetValue(value); }

        public IIdentity Conversation { get => GetValue<IIdentity>(); set => SetValue(value); }

        public IIdentity ConversationIdentityType { get => GetValue<IIdentity>(); set => SetValue(value); }

        public IIdentity Device { get => GetValue<IIdentity>(); set => SetValue(value); }

        public IIdentity Phone { get => GetValue<IIdentity>(); set => SetValue(value); }

        public IIdentity User { get => GetValue<IIdentity>(); set => SetValue(value); }
    }
}
