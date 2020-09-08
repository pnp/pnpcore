namespace PnP.Core.Model.Teams
{
    [GraphType]
    internal partial class TeamMembersSettings : BaseComplexType<ITeamMembersSettings>, ITeamMembersSettings
    {
        public bool AllowCreateUpdateChannels { get => GetValue<bool>(); set => SetValue(value); }

        public bool AllowCreatePrivateChannels { get => GetValue<bool>(); set => SetValue(value); }

        public bool AllowDeleteChannels { get => GetValue<bool>(); set => SetValue(value); }
        
        public bool AllowAddRemoveApps { get => GetValue<bool>(); set => SetValue(value); }
        
        public bool AllowCreateUpdateRemoveTabs { get => GetValue<bool>(); set => SetValue(value); }
        
        public bool AllowCreateUpdateRemoveConnectors { get => GetValue<bool>(); set => SetValue(value); }
    }
}
