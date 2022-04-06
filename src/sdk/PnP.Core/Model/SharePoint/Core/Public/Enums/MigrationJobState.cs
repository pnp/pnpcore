namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// Specifies the state of the migration job
    /// </summary>
    public enum MigrationJobState
    {
        /// <summary>
        /// Finished
        /// </summary>
        None = 0,

        /// <summary>
        /// In progress
        /// </summary>
        Processing = 4,
        
        /// <summary>
        /// In queue to being processed
        /// </summary>
        Queued = 2
    }
}
