namespace PnP.Core.Admin.Model.Microsoft365
{
    /// <summary>
    /// Options to control a creation flow
    /// </summary>
    public class CreationOptions
    {
        /// <summary>
        /// Number of times to check for provisioning complete status. Defaults to 10 if not set.
        /// </summary>
        public int? MaxStatusChecks { get; set; }

        /// <summary>
        /// Defines the wait time in seconds after a status check. Defaults to 1 if not set.
        /// </summary>
        public int? WaitAfterStatusCheck { get; set; }
    }
}
