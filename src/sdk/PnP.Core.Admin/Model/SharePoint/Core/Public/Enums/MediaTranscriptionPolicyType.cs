namespace PnP.Core.Admin.Model.SharePoint
{
    /// <summary>
    /// Defines the media transcription policy type values
    /// </summary>
    public enum MediaTranscriptionPolicyType
    {
        /// <summary>
        /// When enabled, all users on the tenant are able to request transcription on all
        /// media files they have edit permissions to
        /// </summary>
        Enabled = 0,

        /// <summary>
        /// Transcription is disabled
        /// </summary>
        Disabled = 1,
    }
}
