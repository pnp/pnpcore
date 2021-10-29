namespace PnP.Core.Admin.Model.SharePoint
{
    /// <summary>
    /// Specifies the state of Workflows 2013 in the tenant
    /// </summary>
    public enum Workflows2013State
    {
        /// <summary>
        /// Workflow 2013 is disabled
        /// </summary>
        Disabled = 0,

        /// <summary>
        /// The workflow 2013 status is being configured
        /// </summary>
        Configuring = 1,

        /// <summary>
        /// Workflow 2013 is enabled
        /// </summary>
        Enabled = 2
    }
}
