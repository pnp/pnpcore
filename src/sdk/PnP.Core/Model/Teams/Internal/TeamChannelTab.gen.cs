using System;

namespace PnP.Core.Model.Teams
{
    internal partial class TeamChannelTab : BaseDataModel<ITeamChannelTab>, ITeamChannelTab
    {
        public Guid Id { get => GetValue<Guid>(); set => SetValue(value); }

        public string DisplayName { get => GetValue<string>(); set => SetValue(value); }

        public Uri WebUrl { get => GetValue<Uri>(); set => SetValue(value); }

        public string SortOrderIndex { get => GetValue<string>(); set => SetValue(value); }

        public ITeamChannelTabConfiguration Configuration { get => GetModelValue<ITeamChannelTabConfiguration>(); set => SetModelValue(value); }

        public ITeamApp TeamsApp { get => GetModelValue<ITeamApp>(); }

        [KeyProperty(nameof(Id))]
        public override object Key { get => this.Id; set => this.Id = Guid.Parse(value.ToString()); }
    }
}
