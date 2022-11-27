namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// Operation types for the SetAccessRequest method
    /// </summary>
    public enum AccessRequestOption
    {
        /// <summary>
        /// Enable the access request
        /// </summary>
        Enabled = 0,

        /// <summary>
        /// Enables the access request for a specific email address. Works in combination with the email parameter.
        /// </summary>
        SpecificMail = 1,

        /// <summary>
        /// Disables the access request and removes the specific email address (if present)
        /// </summary>
        Disabled = 2
    }
}
