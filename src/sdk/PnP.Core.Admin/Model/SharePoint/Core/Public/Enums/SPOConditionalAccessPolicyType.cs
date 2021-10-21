namespace PnP.Core.Admin.Model.SharePoint
{
    /// <summary>
    /// Specifies what type of SPO conditional access policy is enabled for the tenant
    /// </summary>
    public enum SPOConditionalAccessPolicyType
    {
        /// <summary>
        /// No conditional access policy is enabled for the tenant.
        /// No AAD policy is enabled and both SPO-BlockDownloadOfViewableFilesOnUnmanagedDevicesPolicy and 
        /// SPO-BlockDownloadOfAllFilesOnUnmanagedDevicesPolicy in SPO are disabled.
        /// End users have the full access to the content.
        /// </summary>
        AllowFullAccess = 0,

        /// <summary>
        /// Limited access is enabled for the tenant.
        /// We create AAD limited access policy and enable SPO-BlockDownloadOfViewableFilesOnUnmanagedDevicesPolicy in SPO. 
        /// End users get limited access experience when they access to the content.
        /// </summary>
        AllowLimitedAccess = 1,

        /// <summary>
        /// Block access is enabled for the tenant.
        /// We create AAD block access policy and enable SPO-BlockDownloadOfAllFilesOnUnmanagedDevicesPolicy in SPO. 
        /// End users would be blocked sign-in by AAD.
        /// </summary>
        BlockAccess = 2,

        /// <summary>
        /// Authentication Context is enabled for the site.
        /// We create AAD Authentication context policy and enable it in SPO by running this command:
        /// Set-SPOSite - ConditionalAccessPolicy AuthenticationContext -AuthenticationContextName "Contos MFA"
        /// The user will be redirected to AAD to evaluate this policy if
        /// the policy is enabled.
        /// </summary>
        AuthenticationContext = 3
    }
}
