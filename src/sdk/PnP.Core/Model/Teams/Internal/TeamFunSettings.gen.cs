using Microsoft.Extensions.Logging;

namespace PnP.Core.Model.Teams
{
    [GraphType]
    internal partial class TeamFunSettings : BaseComplexType<ITeamFunSettings>, ITeamFunSettings
    {

        public TeamFunSettings()
        {
            MappingHandler = (FromJson input) =>
            {
                switch (input.TargetType.Name)
                {
                    case "TeamGiphyContentRating": return ToEnum<TeamGiphyContentRating>(input.JsonElement);
                }

                input.Log.LogDebug($"Field {input.FieldName} could not be mapped when converting from JSON");

                return null;
            };
        }

        public bool AllowGiphy { get => GetValue<bool>(); set => SetValue(value); }
        public TeamGiphyContentRating GiphyContentRating { get => GetValue<TeamGiphyContentRating>(); set => SetValue(value); }
        public bool AllowStickersAndMemes { get => GetValue<bool>(); set => SetValue(value); }
        public bool AllowCustomMemes { get => GetValue<bool>(); set => SetValue(value); }
    }
}
