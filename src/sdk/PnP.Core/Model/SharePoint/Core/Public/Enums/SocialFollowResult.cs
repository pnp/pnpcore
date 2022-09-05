namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// The result of the follow operation
    /// </summary>
    public enum SocialFollowResult
    {
        /// <summary>
        /// The status is OK
        /// </summary>
        Ok = 0,

        /// <summary>
        /// The content is already followed
        /// </summary>
        AlreadyFollowing = 1,

        /// <summary>
        /// The following limit is reached
        /// </summary>
        LimitReached = 2,

        /// <summary>
        /// Something went wrong when follwoing an item
        /// </summary>
        InternalError = 3,
    }
}
