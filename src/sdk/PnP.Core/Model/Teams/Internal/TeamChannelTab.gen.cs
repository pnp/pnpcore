using System;

namespace PnP.Core.Model.Teams
{
    internal partial class TeamChannelTab : BaseDataModel<ITeamChannelTab>, ITeamChannelTab
    {
        private bool appInstantiated = false;

        public Guid Id { get => GetValue<Guid>(); set => SetValue(value); }

        public string DisplayName { get => GetValue<string>(); set => SetValue(value); }

        public Uri WebUrl { get => GetValue<Uri>(); set => SetValue(value); }

        public ITeamChannelTabConfiguration Configuration { get => GetValue<ITeamChannelTabConfiguration>(); set => SetValue(value); }

        public ITeamApp TeamsApp 
        {
            get
            {
                if (!appInstantiated)
                {
                    var teamApp = new TeamApp
                    {
                        PnPContext = this.PnPContext,
                        Parent = this,
                    };
                    SetValue(teamApp);
                    appInstantiated = true;
                }
                return GetValue<ITeamApp>();
            }
            set
            {
                appInstantiated = true;
                SetValue(value);
            }
        }

        [KeyProperty("Id")]
        public override object Key { get => this.Id; set => this.Id = Guid.Parse(value.ToString()); }
    }
}
