using PnP.Core.Model.Security;

namespace PnP.Core.Model.Teams
{
    [GraphType]
    internal sealed class TeamIdentitySet : BaseDataModel<ITeamIdentitySet>, ITeamIdentitySet
    {
        #region Properties
        public IIdentity Application { get => GetModelValue<IIdentity>(); }

        public IIdentity Conversation { get => GetModelValue<IIdentity>(); }

        public IIdentity ConversationIdentityType { get => GetModelValue<IIdentity>(); }

        public IIdentity Device { get => GetModelValue<IIdentity>(); }

        public IIdentity Phone { get => GetModelValue<IIdentity>(); }

        public IIdentity User { get => GetModelValue<IIdentity>(); }
        #endregion
    }
}
