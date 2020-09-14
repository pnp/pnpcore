namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// Specifies the Recycle Bin stage of the Recycle Bin item.
    /// </summary>
    public enum RecycleBinItemState
    {
        /// <summary>
        /// The stage of the Recycle Bin item is not specified. The value = 0.
        /// </summary>
        None = 0,
        /// <summary>
        /// Specifies that the Recycle Bin item is in the user Recycle Bin. The value = 1.
        /// </summary>
        FirstStageRecycleBin = 1,
        /// <summary>
        /// Specifies that the Recycle Bin Item is in the site collection Recycle Bin. The value = 2.
        /// </summary>
        SecondStageRecycleBin = 2
    }
}
