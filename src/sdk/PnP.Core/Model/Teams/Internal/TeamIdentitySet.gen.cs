using PnP.Core.Model.Security;

namespace PnP.Core.Model.Teams
{
    [GraphType]
    internal partial class TeamIdentitySet : BaseDataModel<ITeamIdentitySet>, ITeamIdentitySet
    {
        //public IIdentity Application { get => GetValue<IIdentity>(); set => SetValue(value); }
        public IIdentity Application
        {
            get
            {
                if (!NavigationPropertyInstantiated())
                {
                    var teamChannelTabConfiguration = new Identity
                    {
                        PnPContext = this.PnPContext,
                        Parent = this,
                    };
                    SetValue(teamChannelTabConfiguration);
                    InstantiateNavigationProperty();
                }
                return GetValue<IIdentity>();
            }
            set
            {
                InstantiateNavigationProperty();
                SetValue(value);
            }
        }

        //public IIdentity Conversation { get => GetValue<IIdentity>(); set => SetValue(value); }
        public IIdentity Conversation
        {
            get
            {
                if (!NavigationPropertyInstantiated())
                {
                    var teamChannelTabConfiguration = new Identity
                    {
                        PnPContext = this.PnPContext,
                        Parent = this,
                    };
                    SetValue(teamChannelTabConfiguration);
                    InstantiateNavigationProperty();
                }
                return GetValue<IIdentity>();
            }
            set
            {
                InstantiateNavigationProperty();
                SetValue(value);
            }
        }

        //public IIdentity ConversationIdentityType { get => GetValue<IIdentity>(); set => SetValue(value); }
        public IIdentity ConversationIdentityType
        {
            get
            {
                if (!NavigationPropertyInstantiated())
                {
                    var teamChannelTabConfiguration = new Identity
                    {
                        PnPContext = this.PnPContext,
                        Parent = this,
                    };
                    SetValue(teamChannelTabConfiguration);
                    InstantiateNavigationProperty();
                }
                return GetValue<IIdentity>();
            }
            set
            {
                InstantiateNavigationProperty();
                SetValue(value);
            }
        }

        //public IIdentity Device { get => GetValue<IIdentity>(); set => SetValue(value); }
        public IIdentity Device
        {
            get
            {
                if (!NavigationPropertyInstantiated())
                {
                    var teamChannelTabConfiguration = new Identity
                    {
                        PnPContext = this.PnPContext,
                        Parent = this,
                    };
                    SetValue(teamChannelTabConfiguration);
                    InstantiateNavigationProperty();
                }
                return GetValue<IIdentity>();
            }
            set
            {
                InstantiateNavigationProperty();
                SetValue(value);
            }
        }

        //public IIdentity Phone { get => GetValue<IIdentity>(); set => SetValue(value); }
        public IIdentity Phone
        {
            get
            {
                if (!NavigationPropertyInstantiated())
                {
                    var teamChannelTabConfiguration = new Identity
                    {
                        PnPContext = this.PnPContext,
                        Parent = this,
                    };
                    SetValue(teamChannelTabConfiguration);
                    InstantiateNavigationProperty();
                }
                return GetValue<IIdentity>();
            }
            set
            {
                InstantiateNavigationProperty();
                SetValue(value);
            }
        }

        //public IIdentity User { get => GetValue<IIdentity>(); set => SetValue(value); }
        public IIdentity User
        {
            get
            {
                if (!NavigationPropertyInstantiated())
                {
                    var teamChannelTabConfiguration = new Identity
                    {
                        PnPContext = this.PnPContext,
                        Parent = this,
                    };
                    SetValue(teamChannelTabConfiguration);
                    InstantiateNavigationProperty();
                }
                return GetValue<IIdentity>();
            }
            set
            {
                InstantiateNavigationProperty();
                SetValue(value);
            }
        }
    }
}
