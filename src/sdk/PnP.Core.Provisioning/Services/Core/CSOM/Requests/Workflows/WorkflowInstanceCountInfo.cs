namespace PnP.Core.Provisioning.Model
{
    /// <summary>
    /// The number of running instances of a workflow association.
    /// </summary>
    internal sealed class WorkflowInstanceCountInfo
    {
        /// <summary>
        /// How many instances are currently running.
        /// </summary>
        internal int Count { get; set; }
    }
}
