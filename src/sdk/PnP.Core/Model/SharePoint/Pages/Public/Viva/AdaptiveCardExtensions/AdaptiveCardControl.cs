namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// ACE card control
    /// </summary>
    public class AdaptiveCardControl
    {
        /// <summary>
        /// Control type
        /// </summary>
        public string Type { get; set; }

        /// <summary>
        /// Control size
        /// </summary>
        public string Size { get; set; }

        /// <summary>
        /// Control weight
        /// </summary>
        public string Weight { get; set; }

        /// <summary>
        /// Control text
        /// </summary>
        public string Text { get; set; }

        /// <summary>
        /// Wrap text in control
        /// </summary>
        public bool Wrap { get; set; }
    }
}
