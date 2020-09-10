namespace PnP.Core.Model.Security
{
    [GraphType]
    internal partial class Identity : BaseComplexType<IIdentity>, IIdentity
    {
        public string Id { get => GetValue<string>(); set => SetValue(value); }

        public string DisplayName { get => GetValue<string>(); set => SetValue(value); }
        
        public string TenantId { get => GetValue<string>(); set => SetValue(value); }

        public string UserIdentityType { get => GetValue<string>(); set => SetValue(value); }
    }
}
