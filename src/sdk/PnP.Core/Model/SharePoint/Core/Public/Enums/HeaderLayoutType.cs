namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// Available header layouts for modern sites
    /// </summary>
    public enum HeaderLayoutType
    {
        /// <summary>
        /// use the original header [deprecated, now reverts to large/standard] ( Value = 0 )
        /// </summary>
        None = 0,
        /// <summary>
        /// large/standard header ( Value = 1 )
        /// </summary>
        Standard = 1,
        /// <summary>
        /// medium/compact header ( Value = 2 )
        /// </summary>
        Compact = 2,
        /// <summary>
        /// small/minimal header [not currently used] ( Value = 3 )
        /// </summary>
        Minimal = 3,
        /// <summary>
        /// extended header ( Value = 4 )
        /// </summary>
        Extended = 4,
    }
}
