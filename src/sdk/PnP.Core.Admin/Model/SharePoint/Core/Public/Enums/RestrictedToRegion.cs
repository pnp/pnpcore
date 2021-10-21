namespace PnP.Core.Admin.Model.SharePoint
{
    /// <summary>
    /// Represents the status of RestrictedToRegion on a site collection
    /// </summary>
    public enum RestrictedToRegion
    {
        /// <summary>
        /// No restrictions set
        /// </summary>
        NoRestriction = 0,

        /// <summary>
        /// Only site move is blocked
        /// </summary>
        BlockMoveOnly = 1,

        /// <summary>
        /// Site move is blocked, also full file content is not cached cross-region
        /// </summary>
        BlockFull = 2,

        /// <summary>
        /// Default value, means the property was never set
        /// </summary>
        Unknown = 3
    }
}
