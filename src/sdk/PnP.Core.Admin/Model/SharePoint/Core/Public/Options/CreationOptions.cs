namespace PnP.Core.Admin.Model.SharePoint
{
    /// <summary>
    /// Options to control a creation flow
    /// </summary>
    public class CreationOptions
    {
        /// <summary>
        /// Number of times to check for provisioning complete status. Defaults to 12 if not set.
        /// </summary>
        public int? MaxStatusChecks { get; set; }

        /// <summary>
        /// Defines the wait time in seconds after a status check. Defaults to 10 if not set.
        /// </summary>
        public int? WaitAfterStatusCheck { get; set; }

        /// <summary>
        /// The provisioning code will work differently if Application permissions are used. Defaults to live checking of the 
        /// current access token if not set.
        /// </summary>
        public bool? UsingApplicationPermissions { get; set; }
    }
}
