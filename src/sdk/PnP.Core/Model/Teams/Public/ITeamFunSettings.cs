namespace PnP.Core.Model.Teams
{
    /// <summary>
    /// Public interface to define the fun settings for a Team
    /// </summary>
    public interface ITeamFunSettings: IComplexType
    {
        /// <summary>
        /// Defines whether the Giphy are allowed in the Team
        /// </summary>
        public bool AllowGiphy { get; set; }

        /// <summary>
        /// Defines the Giphy content rating (strict or moderate)
        /// </summary>
        public TeamGiphyContentRating GiphyContentRating { get; set; }

        /// <summary>
        /// Defines whether the stickers and meme are allowed in the Team
        /// </summary>
        public bool AllowStickersAndMemes { get; set; }

        /// <summary>
        /// Defines whether the custom memes are allowed in the Team
        /// </summary>
        public bool AllowCustomMemes { get; set; }
    }

    /// <summary>
    /// Giphy content rating for giphies being used in a team
    /// </summary>
    public enum TeamGiphyContentRating
    {
        Moderate,
        Strict
    }
}
