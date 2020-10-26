namespace PnP.Core.Model.Security
{
    [GraphType]
    internal partial class Identity : BaseDataModel<IIdentity>, IIdentity
    {
        public string Id { get => GetValue<string>(); set => SetValue(value); }

        public string DisplayName { get => GetValue<string>(); set => SetValue(value); }
        
        public string TenantId { get => GetValue<string>(); set => SetValue(value); }

        public string UserIdentityType { get => GetValue<string>(); set => SetValue(value); }

        [KeyProperty(nameof(Id))]
        public override object Key { get => this.Id; set => this.Id = value.ToString(); }

    }
}
