using PnP.Core.Services;
using System;
using System.Dynamic;
using System.Text.Json;

namespace PnP.Core.Model.Teams
{
    [GraphType]
    internal sealed class TeamChannelIdentity : BaseDataModel<ITeamChannelIdentity>, ITeamChannelIdentity
    {
        #region Properties

        public string ChannelId { get => GetValue<string>(); set => SetValue(value); }

        public string TeamId { get => GetValue<string>(); set => SetValue(value); }
       
        #endregion
    }
}
