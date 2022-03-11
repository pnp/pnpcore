namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// Represents Teams App ACE
    /// </summary>
    public class TeamsACE: AdaptiveCardExtension<TeamsACEProperties>
    {
        /// <summary>
        /// Default constructor
        /// </summary>
        public TeamsACE(CardSize cardSize = CardSize.Medium) : base(cardSize)
        {
            Id = VivaDashboard.DefaultACEToId(DefaultACE.TeamsApp);
            Title = "Teams App";
            Description = "When a user selects this card, it will open a Teams app. Select from a variety of Personal apps or Bots by searching for the one you want to use.";
        }
    }
}
