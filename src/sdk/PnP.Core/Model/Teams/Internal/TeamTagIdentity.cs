using PnP.Core.Model.Security;

namespace PnP.Core.Model.Teams
{
    [GraphType]
    internal sealed class TeamTagIdentity : BaseDataModel<ITeamTagIdentity>, ITeamTagIdentity
    {
        #region Properties
        public string DisplayName { get => GetValue<string>(); set => SetValue(value); }

        public string Id { get => GetValue<string>(); set => SetValue(value); }
        #endregion
    }
}
