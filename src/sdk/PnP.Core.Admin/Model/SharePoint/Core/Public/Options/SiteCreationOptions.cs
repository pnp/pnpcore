namespace PnP.Core.Admin.Model.SharePoint
{
    /// <summary>
    /// Options to control the site collection creation flow
    /// </summary>
    public class SiteCreationOptions: CreationOptions
    {
        /// <summary>
        /// Waits for the async provisioning options to be completed. Does not apply to classic site collection creation
        /// </summary>
        public bool? WaitForAsyncProvisioning { get; set; }

        /// <summary>
        /// Number of times to check for the async provisioning to be completed. Defaults to 80 if not set.
        /// </summary>
        public int? MaxAsyncProvisioningStatusChecks { get; set; }

        /// <summary>
        /// Defines the wait time in seconds after an async provisioning complete check. Defaults to 15 if not set.
        /// </summary>
        public int? WaitAfterAsyncProvisioningStatusCheck { get; set; }

        /// <summary>
        /// Defines the wait time in seconds after the site collection creation call returns. If specified this overrides the <see cref="WaitForAsyncProvisioning"/> setting.
        /// </summary>
        public int? WaitAfterCreation { get; set; }

    }
}
