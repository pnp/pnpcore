namespace PnP.Core.Model.Teams
{
    /// <summary>
    /// Chat message content types
    /// </summary>
    public enum ChatMessageContentType
    {
        /// <summary>
        /// Chat message uses text only
        /// </summary>
        Text,

        /// <summary>
        /// Chat message uses html
        /// </summary>
        Html,

        /// <summary>
        /// Chat message to use adaptive card
        /// </summary>
        AdaptiveCard
    }
}
