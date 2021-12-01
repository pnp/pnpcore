namespace PnP.Core.Model.Security
{
    [GraphType]
    internal sealed class Identity : BaseDataModel<IIdentity>, IIdentity
    {
        #region Properties
        public string Id { get => GetValue<string>(); set => SetValue(value); }

        public string DisplayName { get => GetValue<string>(); set => SetValue(value); }

        public string TenantId { get => GetValue<string>(); set => SetValue(value); }

        public string UserIdentityType { get => GetValue<string>(); set => SetValue(value); }

        [KeyProperty(nameof(Id))]
        public override object Key { get => Id; set => Id = value.ToString(); }
        #endregion
    }
}
