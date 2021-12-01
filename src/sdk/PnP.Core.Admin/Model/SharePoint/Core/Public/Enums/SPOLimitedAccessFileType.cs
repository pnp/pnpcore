namespace PnP.Core.Admin.Model.SharePoint
{
    /// <summary>
    /// Specifies what type of files can be viewed when the AllowLimitedAccess is set as the ConditionalAccessPolicy
    /// </summary>
    public enum SPOLimitedAccessFileType
    {
        /// <summary>
        /// Users affected by the limited access policy can only view Office Online files
        /// </summary>
        OfficeOnlineFilesOnly = 0,

        /// <summary>
        /// Users affected by the limited access policy can view Office Online files and
        /// files which are previewed through the media service
        /// </summary>
        WebPreviewableFiles = 1,

        /// <summary>
        /// Users affected by the limited access policy can view all files types with
        /// browser viewers and download any files without previewers
        /// </summary>
        OtherFiles = 2,
    }
}
