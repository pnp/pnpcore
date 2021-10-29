namespace PnP.Core.Admin.Model.SharePoint
{
    /// <summary>
    /// Specifies what channel meeting recording permission type is enabled for the tenant
    /// </summary>
    public enum ChannelMeetingRecordingPermissionType
    {
        /// <summary>
        /// The Teams Channel Meeting Recording is stored in the default "Recordings" folder.
        /// Permissions are inherited, which by default allow the users within the channel
        /// to edit the video
        /// </summary>
        Editable = 1,

        /// <summary>
        /// The Teams Channel Meeting Recording is stored in the "Recordings/View Only" folder.
        /// The View Only folder has Restricted Reader role assignments for Site Members and
        /// Visitors preventing editing or download
        /// </summary>
        ViewOnly = 2
    }
}
