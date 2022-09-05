namespace PnP.Core.Admin.Model.SharePoint
{
    /// <summary>
    /// Contains the values of the 2 allowed states for MarkNewFileSensitiveByDefault
    /// </summary>
    public enum SensitiveByDefaultState
    {
        /// <summary>
        /// Allows external sharing, default
        /// </summary>
        AllowExternalSharing = 0,

        /// <summary>
        /// Blocks external sharing
        /// </summary>
        BlockExternalSharing
    }
}
