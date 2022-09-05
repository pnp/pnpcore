using PnP.Core.Model.Teams;

namespace PnP.Core.Model.Security
{
    [GraphType]
    internal sealed class Identity : BaseDataModel<IIdentity>, IIdentity
    {
        #region Properties
        public string Id { get => GetValue<string>(); set => SetValue(value); }

        public string DisplayName { get => GetValue<string>(); set => SetValue(value); }

        public string Email { get => GetValue<string>(); set => SetValue(value); }

        public string TenantId { get => GetValue<string>(); set => SetValue(value); }

        public TeamUserIdentityType UserIdentityType { get => GetValue<TeamUserIdentityType>(); set => SetValue(value); }

        [KeyProperty(nameof(Id))]
        public override object Key { get => Id; set => Id = value.ToString(); }
        #endregion
    }
}
