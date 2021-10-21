namespace PnP.Core.Admin.Model.SharePoint
{
    /// <summary>
    /// Specifies the types of files that can be displayed when the block download links feature is being used
    /// </summary>
    public enum BlockDownloadLinksFileTypes
    {
        /// <summary>
        /// Web previewable files only
        /// </summary>
        WebPreviewableFiles = 1,
        
        /// <summary>
        /// Server rendered files only
        /// </summary>
        ServerRenderedFilesOnly = 2
    }
}
