namespace PnP.Core.Admin.Model.Teams
{
    /// <summary>
    /// Options to control the Teams creation flow
    /// </summary>
    public class TeamCreationOptions
    {
        /// <summary>
        /// Number of times to check for provisioning complete status. Defaults to 12 if not set.
        /// </summary>
        public int? MaxStatusChecks { get; set; }

        /// <summary>
        /// Defines the wait time in seconds after a status check. Defaults to 10 if not set.
        /// </summary>
        public int? WaitAfterStatusCheck { get; set; }
    }
}
