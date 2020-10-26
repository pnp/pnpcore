using Microsoft.Extensions.Logging;
using PnP.Core.Services;

namespace PnP.Core.Model.Teams
{
    [GraphType]
    internal partial class TeamFunSettings : BaseDataModel<ITeamFunSettings>, ITeamFunSettings
    {

        public TeamFunSettings()
        {
        }

        public bool AllowGiphy { get => GetValue<bool>(); set => SetValue(value); }
        public TeamGiphyContentRating GiphyContentRating { get => GetValue<TeamGiphyContentRating>(); set => SetValue(value); }
        public bool AllowStickersAndMemes { get => GetValue<bool>(); set => SetValue(value); }
        public bool AllowCustomMemes { get => GetValue<bool>(); set => SetValue(value); }
    }
}
